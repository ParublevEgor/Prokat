namespace Prokat.API.Services
{
    /// <summary>
    /// Тарифы отдельной покупки ски-пасса (без даты посещения), как на прайс-листе курорта.
    /// </summary>
    public static class SkipPassStandalonePricing
    {
        /// <summary>
        /// Сопоставляет фактическую длительность аренды (часы) со строкой прайса ски-пасса «по времени».
        /// Для 1–2 ч аренды берётся тариф 2 ч; для «весь день» (12 ч) — тариф «день».
        /// </summary>
        public static string TimeSlotFromBillableHours(int billableHours)
        {
            var h = Math.Max(1, billableHours);
            if (h >= 12) return "day";
            if (h >= 4) return "4";
            if (h >= 3) return "3";
            return "2";
        }

        public static decimal GetPrice(bool weekend, string mode, string? timeSlot, int? liftCount)
        {
            mode = (mode ?? "").Trim().ToLowerInvariant();
            if (mode == "time")
            {
                var slot = (timeSlot ?? "").Trim().ToLowerInvariant();
                return weekend ? slot switch
                {
                    "2" => 1300m,
                    "3" => 1500m,
                    "4" => 1650m,
                    "day" => 2000m,
                    _ => throw new InvalidOperationException("Выберите длительность ски-пасса: 2 ч, 3 ч, 4 ч или день.")
                } : slot switch
                {
                    "2" => 1000m,
                    "3" => 1100m,
                    "4" => 1150m,
                    "day" => 1350m,
                    _ => throw new InvalidOperationException("Выберите длительность ски-пасса: 2 ч, 3 ч, 4 ч или день.")
                };
            }

            if (mode == "lifts")
            {
                var n = liftCount ?? throw new InvalidOperationException("Укажите количество подъёмов.");
                return weekend ? n switch
                {
                    15 => 1550m,
                    30 => 2500m,
                    50 => 3650m,
                    100 => 6000m,
                    _ => throw new InvalidOperationException("Допустимые пакеты подъёмов: 15, 30, 50, 100.")
                } : n switch
                {
                    15 => 1100m,
                    30 => 1850m,
                    50 => 2650m,
                    100 => 6000m,
                    _ => throw new InvalidOperationException("Допустимые пакеты подъёмов: 15, 30, 50, 100.")
                };
            }

            throw new InvalidOperationException("Выберите тип ски-пасса: по времени или по числу подъёмов.");
        }
    }
}
