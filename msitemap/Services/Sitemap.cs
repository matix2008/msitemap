using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq; // добавлен using для XElement и XDocument
using msitemap.Models;

namespace msitemap.Services
{
    /// <summary>
    /// Класс для генерации sitemap.xml на основе списка страниц и конфигурации.
    /// </summary>
    public class Sitemap
    {
        // Список конфигураций для разных part
        private readonly List<ConfigEntry> _configEntries;
        // Каталог, в который будет сохранён sitemap.xml
        private readonly string _outputDirectory;

        /// <summary>
        /// Конструктор. Загружает и парсит config.json.
        /// </summary>
        /// <param name="configPath">Путь к config.json</param>
        /// <param name="outputDirectory">Каталог для sitemap.xml (по умолчанию — текущий)</param>
        /// <exception cref="FileNotFoundException">Если config.json не найден</exception>
        /// <exception cref="Exception">Если config.json пуст или невалиден</exception>
        public Sitemap(string configPath, string? outputDirectory = null)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Config-файл не найден: {configPath}");
            var configText = File.ReadAllText(configPath);
            _configEntries = JsonSerializer.Deserialize<List<ConfigEntry>>(configText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
                ?? throw new Exception("Config-файл пуст или невалиден");
            if (_configEntries.Count == 0)
                throw new Exception("Config-файл не содержит ни одной записи");
            // Строгая валидация
            ConfigValidator.Validate(_configEntries);
            _outputDirectory = outputDirectory ?? Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Генерирует sitemap.xml на основе списка страниц и сохраняет его в указанный файл.
        /// </summary>
        /// <param name="list">Список страниц (JsonList)</param>
        /// <param name="sitemapFileName">Имя файла sitemap (например, sitemap.xml)</param>
        /// <returns>Количество записей в sitemap</returns>
        public int Make(JsonList list, string sitemapFileName)
        {
            var sitemapEntries = new List<SitemapEntry>();
            // Для каждой страницы ищем соответствующий config и формируем SitemapEntry
            foreach (var item in list.Items)
            {
                var config = _configEntries.FirstOrDefault(c => c.Part == item.Part);
                if (config == null)
                {
                    // Если для part нет конфигурации — пропускаем
                    continue;
                }
                var entry = new SitemapEntry
                {
                    // Формируем Loc как /part/{значение из config.Loc}
                    Loc = $"/{item.Part}/{GetValueByKey(item, config.Loc)}",
                    Lastmod = string.IsNullOrEmpty(config.Lastmod) || 
                        config.Lastmod.Equals("date", StringComparison.OrdinalIgnoreCase) ? item.Date : config.Lastmod,
                    Changefreq = config.Changefreq,
                    Priority = config.Priority
                };
                sitemapEntries.Add(entry);
            }

            var ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urlset = new XElement(XName.Get("urlset", ns),
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                sitemapEntries.Select(e =>
                    new XElement(XName.Get("url", ns),
                        new XElement(XName.Get("loc", ns), e.Loc),
                        new XElement(XName.Get("lastmod", ns), FormatDate(e.Lastmod)),
                        new XElement(XName.Get("changefreq", ns), e.Changefreq),
                        new XElement(XName.Get("priority", ns), e.Priority.ToString(CultureInfo.InvariantCulture))
                    )
                )
            );
            var sitemapDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
            var sitemapPath = Path.Combine(_outputDirectory, sitemapFileName);
            sitemapDoc.Save(sitemapPath);
            return sitemapEntries.Count;
        }

        /// <summary>
        /// Получить значение свойства PageGroupItem по имени (используется для подстановки loc, lastmod и др.)
        /// </summary>
        private static string GetValueByKey(PageGroupItem item, string key)
        {
            return key.ToLower() switch
            {
                "slug" => item.Slug,
                "part" => item.Part,
                "date" => item.Date,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Преобразовать дату к формату ISO 8601 (yyyy-MM-dd) для lastmod
        /// </summary>
        private static string FormatDate(string date)
        {
            if (DateTime.TryParse(date, out var dt))
                return dt.ToString("yyyy-MM-dd");
            if (DateTime.TryParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("yyyy-MM-dd");
            return date;
        }
    }
}
