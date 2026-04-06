using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.Models;

namespace Prokat.API.Services
{
    public interface IOrderPricingService
    {
        /// <summary>Пересчитывает базовую и итоговую сумму заказа по первой аренде и тарифу (идемпотентно).</summary>
        Task<(decimal базовая, decimal сНдс)> ApplyPricingAsync(int orderId, decimal ставкаНдс, CancellationToken ct = default);
    }

    public class OrderPricingService : IOrderPricingService
    {
        private readonly ApplicationDbContext _db;

        public OrderPricingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(decimal базовая, decimal сНдс)> ApplyPricingAsync(int orderId, decimal ставкаНдс, CancellationToken ct = default)
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

            var tariffs = await _db.PriceTariffs.OrderBy(t => t.Время_аренды).ToListAsync(ct);
            if (tariffs.Count == 0)
                throw new InvalidOperationException("В таблице Цены_на_услуги нет тарифов");

            var tariff = tariffs.FirstOrDefault(t => t.Время_аренды >= billableHours)
                ?? tariffs.OrderByDescending(t => t.Время_аренды).First();

            var isWeekend = rental.ДатаНачала.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var basePrice = isWeekend
                ? tariff.Прокат_выходные_и_праздничные_дни
                : tariff.Прокат_будни;

            if (basePrice is null or <= 0)
                throw new InvalidOperationException("Для выбранного тарифа не задана цена проката");

            var базовая = (decimal)basePrice.Value;
            var сНдс = Math.Round(базовая * (1 + ставкаНдс), 2, MidpointRounding.AwayFromZero);

            order.БазоваяСумма = базовая;
            order.Сумма_оплаты = сНдс;
            await _db.SaveChangesAsync(ct);

            return (базовая, сНдс);
        }
    }
}
