using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;
using Prokat.API.Models;

namespace Prokat.API.Services
{
    public interface IRentalBookingService
    {
        Task<BookingResultDto> CreateBookingAsync(BookingCreateRequest request, CancellationToken ct = default);
    }

    public class RentalBookingService : IRentalBookingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IInventoryAvailabilityService _inventory;
        private readonly IOrderPricingService _pricing;

        public RentalBookingService(
            ApplicationDbContext db,
            IInventoryAvailabilityService inventory,
            IOrderPricingService pricing)
        {
            _db = db;
            _inventory = inventory;
            _pricing = pricing;
        }

        public async Task<BookingResultDto> CreateBookingAsync(BookingCreateRequest request, CancellationToken ct = default)
        {
            if (request.EndDate <= request.StartDate)
                throw new InvalidOperationException("Дата окончания должна быть позже начала");

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var order = new Order();
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            var client = new Client
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

            int inventoryId;
            if (request.InventoryId is int chosen)
            {
                var free = await _inventory.GetFreeAsync(request.EquipmentType, request.StartDate, request.EndDate, ct);
                if (free.All(x => x.Id != chosen))
                    throw new InvalidOperationException("Выбранный инвентарь занят в этом интервале");
                inventoryId = chosen;
            }
            else
            {
                var free = await _inventory.GetFreeAsync(request.EquipmentType, request.StartDate, request.EndDate, ct);
                var first = free.FirstOrDefault() ?? throw new InvalidOperationException("Нет свободного инвентаря на выбранные даты");
                inventoryId = first.Id;
            }

            var overlap = await _db.RentalBookings
                .AnyAsync(r => r.ID_Инвентаря == inventoryId
                    && r.Статус != "Отмена"
                    && r.ДатаНачала < request.EndDate
                    && request.StartDate < r.ДатаОкончания, ct);
            if (overlap)
                throw new InvalidOperationException("Инвентарь уже занят (пересечение интервалов)");

            var rental = new RentalBooking
            {
                ID_Заказа = order.ID_Заказа,
                ID_Инвентаря = inventoryId,
                ДатаНачала = request.StartDate,
                ДатаОкончания = request.EndDate,
                Статус = "Бронь",
            };
            _db.RentalBookings.Add(rental);
            await _db.SaveChangesAsync(ct);

            var (базовая, сНдс) = await _pricing.ApplyPricingAsync(order.ID_Заказа, request.VatRate, ct);

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
