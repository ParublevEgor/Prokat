using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;
using Prokat.API.Models;

namespace Prokat.API.Services
{
    public interface IRentalBookingService
    {
        Task<BookingResultDto> CreateBookingAsync(BookingCreateRequest request, int? appUserId, CancellationToken ct = default);
    }

    public class RentalBookingService : IRentalBookingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IInventoryAvailabilityService _inventory;
        private readonly IOrderPricingService _pricing;
        private readonly ISiteSettingsService _settings;

        public RentalBookingService(
            ApplicationDbContext db,
            IInventoryAvailabilityService inventory,
            IOrderPricingService pricing,
            ISiteSettingsService settings)
        {
            _db = db;
            _inventory = inventory;
            _pricing = pricing;
            _settings = settings;
        }

        public async Task<BookingResultDto> CreateBookingAsync(BookingCreateRequest request, int? appUserId, CancellationToken ct = default)
        {
            if (appUserId is null)
                throw new InvalidOperationException("Войдите в систему, чтобы оформить бронирование.");

            var (startDate, endDate) = RentalWindowHelper.ComputeWindow(request.RentalDate, request.DurationKey);
            if (endDate <= startDate)
                throw new InvalidOperationException("Некорректный интервал аренды.");

            var vatRate = await _settings.GetVatRateAsync(ct);

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var order = new Order();
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            var acc = await _db.AppUsers.FirstOrDefaultAsync(u => u.ID_Учетной_записи == appUserId.Value, ct)
                ?? throw new InvalidOperationException("Учётная запись не найдена.");

            Client client;

            if (acc.Роль == "Admin" && acc.ID_Клиента is null)
            {
                if (string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.FirstName))
                    throw new InvalidOperationException("Укажите фамилию и имя клиента.");

                client = new Client
                {
                    Фамилия = request.LastName,
                    Имя = request.FirstName,
                    Возраст = request.Age,
                    Рост = request.Height,
                    Вес = request.Weight,
                    Залог = request.Deposit,
                    ID_Заказа = order.ID_Заказа,
                };
                _db.Clients.Add(client);
                await _db.SaveChangesAsync(ct);

                order.ID_Клиента = client.ID_Клиента;
                await _db.SaveChangesAsync(ct);
            }
            else if (acc.ID_Клиента is int cid)
            {
                client = await _db.Clients.FirstOrDefaultAsync(c => c.ID_Клиента == cid, ct)
                    ?? throw new InvalidOperationException("Клиент не найден.");

                order.ID_Клиента = cid;
                client.ID_Заказа = order.ID_Заказа;
                await _db.SaveChangesAsync(ct);
            }
            else
                throw new InvalidOperationException("Некорректная учётная запись для бронирования.");

            int inventoryId;
            if (request.InventoryId is int chosen)
            {
                var free = await _inventory.GetFreeAsync(request.EquipmentType, startDate, endDate, ct);
                if (free.All(x => x.Id != chosen))
                    throw new InvalidOperationException("Выбранный инвентарь занят в этом интервале");
                inventoryId = chosen;
            }
            else
            {
                var free = await _inventory.GetFreeAsync(request.EquipmentType, startDate, endDate, ct);
                var first = free.FirstOrDefault() ?? throw new InvalidOperationException("Нет свободного инвентаря на выбранные даты");
                inventoryId = first.Id;
            }

            var overlap = await _db.RentalBookings
                .AnyAsync(r => r.ID_Инвентаря == inventoryId
                    && r.Статус != "Отмена"
                    && r.ДатаНачала < endDate
                    && startDate < r.ДатаОкончания, ct);
            if (overlap)
                throw new InvalidOperationException("Инвентарь уже занят (пересечение интервалов)");

            var rental = new RentalBooking
            {
                ID_Заказа = order.ID_Заказа,
                ID_Инвентаря = inventoryId,
                ДатаНачала = startDate,
                ДатаОкончания = endDate,
                Статус = "Бронь",
            };
            _db.RentalBookings.Add(rental);
            await _db.SaveChangesAsync(ct);

            var (базовая, сНдс) = await _pricing.ApplyPricingAsync(order.ID_Заказа, vatRate, ct);

            await tx.CommitAsync(ct);

            return new BookingResultDto
            {
                OrderId = order.ID_Заказа,
                ClientId = client.ID_Клиента,
                RentalId = rental.ID_Аренды,
                InventoryId = inventoryId,
                BaseAmount = базовая,
                TotalWithVat = сНдс,
            };
        }
    }
}
