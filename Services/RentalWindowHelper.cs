namespace Prokat.API.Services
{
    public static class RentalWindowHelper
    {
        /// <summary>Начало дня проката (час начала смены).</summary>
        public const int DayStartHour = 9;

        /// <summary>Для режима «весь день» — длительность в часах (9:00–21:00).</summary>
        public const int FullDayHours = 12;

        public static (DateTime Start, DateTime End) ComputeWindow(DateTime rentalDate, string durationKey)
        {
            var d = rentalDate.Date;
            var dayStart = new DateTime(d.Year, d.Month, d.Day, DayStartHour, 0, 0, DateTimeKind.Unspecified);
            return durationKey?.Trim().ToLowerInvariant() switch
            {
                "1" or "1h" => (dayStart, dayStart.AddHours(1)),
                "2" or "2h" => (dayStart, dayStart.AddHours(2)),
                "4" or "4h" => (dayStart, dayStart.AddHours(4)),
                "day" or "fullday" => (dayStart, dayStart.AddHours(FullDayHours)),
                _ => throw new InvalidOperationException("Недопустимая длительность: выберите 1, 2, 4 часа или весь день."),
            };
        }
    }
}
