using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.Models;

namespace Prokat.API.Services
{
    public interface IOrderPricingService
    {
        /// <summary>Пересчитывает базовую и итоговую сумму заказа по первой аренде и тарифу (идемпотентно).</summary>
        Task<(decimal базовая, decimal прокат, decimal скипасс, decimal сНдс)> ApplyPricingAsync(
            int orderId,
            decimal ставкаНдс,
            bool includeSkiPass,
            CancellationToken ct = default);
        Task<(decimal прокат, decimal скипасс, decimal базовая, decimal сНдс)> QuoteAsync(
            DateTime start,
            DateTime end,
            decimal ставкаНдс,
            bool includeRental,
            bool includeSkiPass,
            CancellationToken ct = default);
    }

    public class OrderPricingService : IOrderPricingService
    {
        private readonly ApplicationDbContext _db;

        public OrderPricingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(decimal базовая, decimal прокат, decimal скипасс, decimal сНдс)> ApplyPricingAsync(
            int orderId,
            decimal ставкаНдс,
            bool includeSkiPass,
            CancellationToken ct = default)
        {
            var order = await _db.Orders
                .Include(o => o.Rentals)
                .FirstOrDefaultAsync(o => o.ID_Заказа == orderId, ct)
                ?? throw new InvalidOperationException("Заказ не найден");

            var rental = order.Rentals.OrderBy(r => r.ID_Аренды).FirstOrDefault()
                ?? throw new InvalidOperationException("Нет строк аренды для расчёта");

            if (rental.ДатаОкончания <= rental.ДатаНачала)
                throw new InvalidOperationException("Некорректный интервал дат");

            var billableHours = Math.Max(1, (int)Math.Ceiling((rental.ДатаОкончания - rental.ДатаНачала).TotalHours));

            var (прокат, скипасс, базовая, сНдс) = await QuoteAsync(
                rental.ДатаНачала,
                rental.ДатаОкончания,
                ставкаНдс,
                includeRental: true,
                includeSkiPass: includeSkiPass,
                ct: ct);

            order.БазоваяСумма = базовая;
            order.Сумма_оплаты = сНдс;
            await _db.SaveChangesAsync(ct);

            return (базовая, прокат, скипасс, сНдс);
        }

        public async Task<(decimal прокат, decimal скипасс, decimal базовая, decimal сНдс)> QuoteAsync(
            DateTime start,
            DateTime end,
            decimal ставкаНдс,
            bool includeRental,
            bool includeSkiPass,
            CancellationToken ct = default)
        {
            if (end <= start)
                throw new InvalidOperationException("Некорректный интервал дат");

            var billableHours = Math.Max(1, (int)Math.Ceiling((end - start).TotalHours));

            var tariffs = await _db.PriceTariffs.OrderBy(t => t.Время_аренды).ToListAsync(ct);
            if (tariffs.Count == 0)
                throw new InvalidOperationException("В таблице Цены_на_услуги нет тарифов");

            var tariff = tariffs.FirstOrDefault(t => t.Время_аренды >= billableHours)
                ?? tariffs.OrderByDescending(t => t.Время_аренды).First();

            var isWeekend = start.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            decimal rentalPrice = 0m;
            decimal skipassPrice = 0m;

            if (includeRental)
            {
                var rentalRaw = isWeekend
                    ? tariff.Прокат_выходные_и_праздничные_дни
                    : tariff.Прокат_будни;
                if (rentalRaw is null or <= 0)
                    throw new InvalidOperationException("Для выбранного тарифа не задана цена проката");
                rentalPrice = rentalRaw.Value;
            }

            if (includeSkiPass)
            {
                var passRaw = isWeekend
                    ? tariff.Скипасс_выходные_и_праздиничные_дни
                    : tariff.Скипасс_будни;
                if (passRaw is null or <= 0)
                    throw new InvalidOperationException("Для выбранного тарифа не задана цена ски-пасса");
                skipassPrice = passRaw.Value;
            }

            var baseTotal = rentalPrice + skipassPrice;
            var withVat = Math.Round(baseTotal * (1 + ставкаНдс), 2, MidpointRounding.AwayFromZero);
            return (rentalPrice, skipassPrice, baseTotal, withVat);
        }
    }
}
