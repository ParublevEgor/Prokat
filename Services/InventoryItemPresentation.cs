using System.Text.RegularExpressions;
using Prokat.API.DTO;

namespace Prokat.API.Services
{
    /// <summary>Человеко-читаемые подписи для карточек без акцента на бренды.</summary>
    public static class InventoryItemPresentation
    {
        public static void Enrich(InventoryItemDto item, string? equipmentType)
        {
            var kind = (equipmentType ?? "").Trim();
            var isSnowboard = IsSnowboardKind(kind);
            var main = isSnowboard ? item.Snowboard : item.Skis;

            item.SizeClass = InferSizeClass(main);
            item.LengthCmHint = TryParseCm(main);
            item.BootSizeEuHint = ParseBootEu(item.Boots);
            item.CardTitle = BuildCardTitle(isSnowboard, item.LengthCmHint, item.SizeClass);
            item.CardSubtitle = BuildCardSubtitle(item.BootSizeEuHint, item.Helmet, item.Goggles);
            item.ModelReference = BuildModelReference(isSnowboard, item);
        }

        private static bool IsSnowboardKind(string kind) =>
            kind.Equals("Snowboard", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("Сноуборд", StringComparison.OrdinalIgnoreCase);

        private static string? InferSizeClass(string? text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            foreach (var m in new[] { " S ", " S", "(S)", "разм. S", "размер S" })
                if (text.Contains(m.Trim(), StringComparison.OrdinalIgnoreCase)) return "S";
            if (Regex.IsMatch(text, @"\bS\b") && text.IndexOf('S') >= 0) return "S";
            foreach (var m in new[] { " M ", " M", "(M)", "разм. M" })
                if (text.Contains(m.Trim(), StringComparison.OrdinalIgnoreCase)) return "M";
            if (Regex.IsMatch(text, @"\bM\b")) return "M";
            foreach (var m in new[] { " L ", " L", "(L)", "разм. L" })
                if (text.Contains(m.Trim(), StringComparison.OrdinalIgnoreCase)) return "L";
            if (Regex.IsMatch(text, @"\bL\b")) return "L";
            return null;
        }

        private static int? TryParseCm(string? text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var m = Regex.Match(text, @"(\d{2,3})\s*см");
            if (m.Success && int.TryParse(m.Groups[1].Value, out var cm) && cm is >= 80 and <= 220)
                return cm;
            m = Regex.Match(text, @"(\d{3})\s*cm", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out cm) && cm is >= 80 and <= 220)
                return cm;
            return null;
        }

        private static int? ParseBootEu(string? boots)
        {
            if (string.IsNullOrEmpty(boots)) return null;
            var m = Regex.Match(boots, @"(\d{2})");
            return m.Success && int.TryParse(m.Groups[1].Value, out var eu) && eu is >= 33 and <= 52 ? eu : null;
        }

        private static string BuildCardTitle(bool isSnowboard, int? lengthCm, string? sizeClass)
        {
            var typeRu = isSnowboard ? "Сноуборд" : "Лыжи";
            var parts = new List<string> { typeRu };
            if (lengthCm is int len)
                parts.Add($"длина ~{len} см");
            if (!string.IsNullOrEmpty(sizeClass))
                parts.Add($"комплект {sizeClass}");
            return string.Join(" · ", parts);
        }

        private static string? BuildCardSubtitle(int? bootEu, string? helmet, string? goggles)
        {
            var parts = new List<string>();
            if (bootEu is int eu)
                parts.Add($"ботинки EU {eu}");
            var h = ShortSizeLabel(helmet);
            if (!string.IsNullOrEmpty(h))
                parts.Add($"шлем {h}");
            var g = ShortSizeLabel(goggles);
            if (!string.IsNullOrEmpty(g))
                parts.Add($"маска {g}");
            return parts.Count == 0 ? null : string.Join(" · ", parts);
        }

        private static string? ShortSizeLabel(string? raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            foreach (var s in new[] { "S", "M", "L" })
                if (raw.Contains(s, StringComparison.OrdinalIgnoreCase))
                    return s;
            return raw.Length > 12 ? raw[..12] + "…" : raw;
        }

        /// <summary>Техническое имя для раскрытия «Подробнее».</summary>
        private static string? BuildModelReference(bool isSnowboard, InventoryItemDto item)
        {
            var main = isSnowboard ? item.Snowboard : item.Skis;
            return string.IsNullOrWhiteSpace(main) ? null : main.Trim();
        }
    }
}
