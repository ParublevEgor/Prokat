using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;
using Prokat.API.Models;

namespace Prokat.API.Services
{
    public interface IRentalBookingService
    {
        Task<BookingResultDto> CreateBookingAsync(BookingCreateRequest request, int? appUserId, CancellationToken ct = default);
        Task<BookingQuoteDto> QuoteAsync(BookingQuoteRequest request, CancellationToken ct = default);
        Task<SkipPassPurchaseResultDto> CreateSkipPassPurchaseAsync(SkipPassPurchaseRequest request, int? appUserId, CancellationToken ct = default);
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

            var client = await ResolveClientForOrderAsync(
                acc,
                order,
                request.LastName,
                request.FirstName,
                request.Age,
                request.Height,
                request.Weight,
                request.Deposit,
                ct);

            int inventoryId;
            if (request.InventoryId is int chosen)
            {
                var free = await _inventory.GetFreeAsync(request.EquipmentType.ToString(), startDate, endDate, ct);
                if (free.All(x => x.Id != chosen))
                    throw new InvalidOperationException("Выбранный инвентарь занят в этом интервале");
                inventoryId = chosen;
            }
            else
            {
                var free = await _inventory.GetFreeAsync(request.EquipmentType.ToString(), startDate, endDate, ct);
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

            var (базовая, прокат, скипасс, сНдс) = await _pricing.ApplyPricingAsync(
                order.ID_Заказа,
                vatRate,
                request.IncludeSkiPass,
                ct);

            await tx.CommitAsync(ct);

            return new BookingResultDto
            {
                OrderId = order.ID_Заказа,
                ClientId = client.ID_Клиента,
                RentalId = rental.ID_Аренды,
                InventoryId = inventoryId,
                BaseAmount = базовая,
                RentalAmount = прокат,
                SkiPassAmount = скипасс,
                TotalWithVat = сНдс,
            };
        }

        public async Task<BookingQuoteDto> QuoteAsync(BookingQuoteRequest request, CancellationToken ct = default)
        {
            var (startDate, endDate) = RentalWindowHelper.ComputeWindow(request.RentalDate, request.DurationKey);
            var vatRate = await _settings.GetVatRateAsync(ct);
            var (прокат, скипасс, базовая, сНдс) = await _pricing.QuoteAsync(
                startDate,
                endDate,
                vatRate,
                includeRental: true,
                includeSkiPass: request.IncludeSkiPass,
                ct);
            return new BookingQuoteDto
            {
                Start = startDate,
                End = endDate,
                RentalAmount = прокат,
                SkiPassAmount = скипасс,
                BaseAmount = базовая,
                VatRate = vatRate,
                TotalWithVat = сНдс
            };
        }

        public async Task<SkipPassPurchaseResultDto> CreateSkipPassPurchaseAsync(
            SkipPassPurchaseRequest request,
            int? appUserId,
            CancellationToken ct = default)
        {
            if (appUserId is null)
                throw new InvalidOperationException("Войдите в систему, чтобы оформить покупку ски-пасса.");

            var weekend = string.Equals(request.DayKind?.Trim(), "weekend", StringComparison.OrdinalIgnoreCase);
            var mode = string.IsNullOrWhiteSpace(request.Mode) ? "time" : request.Mode.Trim();
            var skipass = SkipPassStandalonePricing.GetPrice(
                weekend,
                mode,
                request.TimeSlot,
                request.LiftCount);
            var vatRate = await _settings.GetVatRateAsync(ct);
            var baseAmount = skipass;
            var total = Math.Round(baseAmount * (1 + vatRate), 2, MidpointRounding.AwayFromZero);
            var now = DateTime.UtcNow;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var order = new Order();
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            var acc = await _db.AppUsers.FirstOrDefaultAsync(u => u.ID_Учетной_записи == appUserId.Value, ct)
                ?? throw new InvalidOperationException("Учётная запись не найдена.");
            var client = await ResolveClientForOrderAsync(
                acc,
                order,
                request.LastName,
                request.FirstName,
                request.Age,
                request.Height,
                request.Weight,
                request.Deposit,
                ct);

            order.БазоваяСумма = baseAmount;
            order.Сумма_оплаты = total;
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new SkipPassPurchaseResultDto
            {
                OrderId = order.ID_Заказа,
                ClientId = client.ID_Клиента,
                Start = now,
                End = now,
                SkiPassAmount = skipass,
                TotalWithVat = total
            };
        }

        private async Task<Client> ResolveClientForOrderAsync(
            AppUser acc,
            Order order,
            string? lastName,
            string? firstName,
            int? age,
            int? height,
            int? weight,
            int? deposit,
            CancellationToken ct)
        {
            if (acc.Роль == "Admin" && acc.ID_Клиента is null)
            {
                if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName))
                    throw new InvalidOperationException("Укажите фамилию и имя клиента.");

                var client = new Client
                {
                    Фамилия = lastName,
                    Имя = firstName,
                    Возраст = age,
                    Рост = height,
                    Вес = weight,
                    Залог = deposit,
                    ID_Заказа = order.ID_Заказа,
                };
                _db.Clients.Add(client);
                await _db.SaveChangesAsync(ct);
                order.ID_Клиента = client.ID_Клиента;
                await _db.SaveChangesAsync(ct);
                return client;
            }

            if (acc.ID_Клиента is int cid)
            {
                var client = await _db.Clients.FirstOrDefaultAsync(c => c.ID_Клиента == cid, ct)
                    ?? throw new InvalidOperationException("Клиент не найден.");
                order.ID_Клиента = cid;
                client.ID_Заказа = order.ID_Заказа;
                await _db.SaveChangesAsync(ct);
                return client;
            }

            throw new InvalidOperationException("Некорректная учётная запись для бронирования.");
        }
    }
}
