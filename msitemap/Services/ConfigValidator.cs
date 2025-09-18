using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using msitemap.Models;

namespace msitemap.Services
{
    /// <summary>
    /// Строгая валидация списка конфигураций для sitemap.
    /// </summary>
    public static class ConfigValidator
    {
        private static readonly string[] AllowedLocLastmod = { "slug", "part", "date" };
        private static readonly string[] AllowedChangefreq = { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };

        public static void Validate(List<ConfigEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                throw new Exception("Config-файл не содержит ни одной записи.");

            var partSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                string prefix = $"Config[{i}]: ";

                // Проверка обязательности
                if (string.IsNullOrWhiteSpace(entry.Part))
                    throw new Exception(prefix + "Поле 'Part' обязательно.");
                if (string.IsNullOrWhiteSpace(entry.Loc))
                    throw new Exception(prefix + "Поле 'Loc' обязательно.");
                if (string.IsNullOrWhiteSpace(entry.Lastmod))
                    throw new Exception(prefix + "Поле 'Lastmod' обязательно.");
                if (string.IsNullOrWhiteSpace(entry.Changefreq))
                    throw new Exception(prefix + "Поле 'Changefreq' обязательно.");
                // Priority: 0.0 допустимо, но не должно быть NaN
                if (double.IsNaN(entry.Priority))
                    throw new Exception(prefix + "Поле 'Priority' обязательно и должно быть числом.");

                // Проверка уникальности Part
                if (!partSet.Add(entry.Part))
                    throw new Exception(prefix + $"Дублирующееся значение 'Part': '{entry.Part}'.");

                // Проверка допустимых значений Loc
                if (!AllowedLocLastmod.Contains(entry.Loc.ToLower()) && string.IsNullOrWhiteSpace(entry.Loc) == false)
                    throw new Exception(prefix + $"Недопустимое значение 'Loc': '{entry.Loc}'. Разрешено: slug, part, date.");

                // Проверка допустимых значений Lastmod
                if (!AllowedLocLastmod.Contains(entry.Lastmod.ToLower()))
                {
                    // Разрешаем только ISO дату или dd.MM.yyyy
                    if (!DateTime.TryParse(entry.Lastmod, out _) &&
                        !DateTime.TryParseExact(entry.Lastmod, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    {
                        throw new Exception(prefix + $"Недопустимое значение 'Lastmod': '{entry.Lastmod}'. Разрешено: slug, part, date или дата.");
                    }
                }

                // Проверка Changefreq
                if (!AllowedChangefreq.Contains(entry.Changefreq.ToLower()))
                    throw new Exception(prefix + $"Недопустимое значение 'Changefreq': '{entry.Changefreq}'. Разрешено: {string.Join(", ", AllowedChangefreq)}.");

                // Проверка Priority
                if (entry.Priority < 0.0 || entry.Priority > 1.0)
                    throw new Exception(prefix + $"Priority должен быть в диапазоне 0.0 ... 1.0.");
            }
        }
    }
}
