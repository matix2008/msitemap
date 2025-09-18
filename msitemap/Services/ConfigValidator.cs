using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using msitemap.Models;

namespace msitemap.Services
{
    /// <summary>
    /// ������� ��������� ������ ������������ ��� sitemap.
    /// </summary>
    public static class ConfigValidator
    {
        private static readonly string[] AllowedLocLastmod = { "slug", "part", "date" };
        private static readonly string[] AllowedChangefreq = { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };

        public static void Validate(List<ConfigEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                throw new Exception("Config-���� �� �������� �� ����� ������.");

            var partSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                string prefix = $"Config[{i}]: ";

                // �������� ��������������
                if (string.IsNullOrWhiteSpace(entry.Part))
                    throw new Exception(prefix + "���� 'Part' �����������.");
                if (string.IsNullOrWhiteSpace(entry.Loc))
                    throw new Exception(prefix + "���� 'Loc' �����������.");
                if (string.IsNullOrWhiteSpace(entry.Lastmod))
                    throw new Exception(prefix + "���� 'Lastmod' �����������.");
                if (string.IsNullOrWhiteSpace(entry.Changefreq))
                    throw new Exception(prefix + "���� 'Changefreq' �����������.");
                // Priority: 0.0 ���������, �� �� ������ ���� NaN
                if (double.IsNaN(entry.Priority))
                    throw new Exception(prefix + "���� 'Priority' ����������� � ������ ���� ������.");

                // �������� ������������ Part
                if (!partSet.Add(entry.Part))
                    throw new Exception(prefix + $"������������� �������� 'Part': '{entry.Part}'.");

                // �������� ���������� �������� Loc
                if (!AllowedLocLastmod.Contains(entry.Loc.ToLower()) && string.IsNullOrWhiteSpace(entry.Loc) == false)
                    throw new Exception(prefix + $"������������ �������� 'Loc': '{entry.Loc}'. ���������: slug, part, date.");

                // �������� ���������� �������� Lastmod
                if (!AllowedLocLastmod.Contains(entry.Lastmod.ToLower()))
                {
                    // ��������� ������ ISO ���� ��� dd.MM.yyyy
                    if (!DateTime.TryParse(entry.Lastmod, out _) &&
                        !DateTime.TryParseExact(entry.Lastmod, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    {
                        throw new Exception(prefix + $"������������ �������� 'Lastmod': '{entry.Lastmod}'. ���������: slug, part, date ��� ����.");
                    }
                }

                // �������� Changefreq
                if (!AllowedChangefreq.Contains(entry.Changefreq.ToLower()))
                    throw new Exception(prefix + $"������������ �������� 'Changefreq': '{entry.Changefreq}'. ���������: {string.Join(", ", AllowedChangefreq)}.");

                // �������� Priority
                if (entry.Priority < 0.0 || entry.Priority > 1.0)
                    throw new Exception(prefix + $"Priority ������ ���� � ��������� 0.0 ... 1.0.");
            }
        }
    }
}
