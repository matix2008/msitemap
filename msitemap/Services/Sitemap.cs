using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using msitemap.Models;

namespace msitemap.Services
{
    /// <summary>
    /// Класс для генерации sitemap.xml на основе списка страниц и конфигурации.
    /// </summary>
    public class Sitemap
    {
        private readonly SitemapConfig _config;
        private readonly string _outputDirectory;

        public Sitemap(string configPath, string? outputDirectory = null)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Config-файл не найден: {configPath}");
            var configText = File.ReadAllText(configPath);
            _config = JsonSerializer.Deserialize<SitemapConfig>(configText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Config-файл пуст или невалиден");
            if (_config.Parts == null || _config.Parts.Count == 0)
                throw new Exception("Config-файл не содержит ни одной записи в parts");
            // Строгая валидация
            ConfigValidator.Validate(_config.Parts);
            _outputDirectory = outputDirectory ?? Directory.GetCurrentDirectory();
        }
        public int Make(JsonList list, string sitemapFileName)
        {
            var sitemapEntries = new List<SitemapEntry>();
            var usedParts = new HashSet<string>();
            string root = _config.Root?.TrimEnd('/') ?? string.Empty;

            int itemsUsed = 0;
            foreach (var cfgPart in _config.Parts)
            {
                // Добавляем самостоятельные части сразу
                if (cfgPart.PartAsSolo)
                {
                    var loc = $"{root}/{cfgPart.Part}".Replace("//", "/").TrimEnd('/');
                    string lastmod;
                    if (string.Equals(cfgPart.Lastmod, "date", StringComparison.OrdinalIgnoreCase))
                        lastmod = DateTime.Now.ToString("yyyy-MM-dd");
                    else
                        lastmod = cfgPart.Lastmod;
                    sitemapEntries.Add(new SitemapEntry
                    {
                        Loc = loc,
                        Lastmod = FormatDate(lastmod),
                        Changefreq = cfgPart.Changefreq,
                        Priority = cfgPart.Priority
                    });
                }

                // Для каждой Part получаем список элементов (PageGroupItem)
                var partItems = list.GetItemsByPart(cfgPart.Part);
                foreach (var item in partItems)
                {
                    var locValue = GetValueByKey(item, cfgPart.Loc);
                    if (string.IsNullOrWhiteSpace(locValue))
                        continue;
                    var loc = $"{root}/{item.Part}{(string.IsNullOrEmpty(item.Part) ? "" : "/")}{locValue}".Replace("//", "/");
                    var lastmod = (string.IsNullOrEmpty(cfgPart.Lastmod) || cfgPart.Lastmod.Equals("date", StringComparison.OrdinalIgnoreCase))
                        ? item.Date
                        : cfgPart.Lastmod;

                    sitemapEntries.Add(new SitemapEntry
                    {
                        Loc = loc,
                        Lastmod = FormatDate(lastmod),
                        Changefreq = cfgPart.Changefreq,
                        Priority = cfgPart.Priority
                    });

                    itemsUsed++;
                }
            }

            if( itemsUsed != list.Items.Count)
                Logger.Log($"Предупреждение: Кол-во использованных исходных элементов ({itemsUsed}) не равно общему кол-ву исходных элеентов ({list.Items.Count}).");

            //// Для каждой страницы ищем соответствующий config и формируем SitemapEntry
            //foreach (var item in list.Items)
            //{
            //    var config = _config.Parts.FirstOrDefault(c => c.Part == item.Part);
            //    if (config == null)
            //        continue;
            //    var locValue = GetValueByKey(item, config.Loc);
            //    if (string.IsNullOrWhiteSpace(locValue))
            //        continue;
            //    var loc = $"{root}/{item.Part}{(string.IsNullOrEmpty(item.Part) ? "" : "/")}{locValue}".Replace("//", "/");
            //    var lastmod = (string.IsNullOrEmpty(config.Lastmod) || config.Lastmod.Equals("date", StringComparison.OrdinalIgnoreCase))
            //        ? item.Date
            //        : config.Lastmod;
            //    sitemapEntries.Add(new SitemapEntry
            //    {
            //        Loc = loc,
            //        Lastmod = FormatDate(lastmod),
            //        Changefreq = config.Changefreq,
            //        Priority = config.Priority
            //    });
            //    usedParts.Add(item.Part);
            //}
            //// Добавляем solo parts, если их нет в usedParts
            //foreach (var config in _config.Parts.Where(p => p.PartAsSolo && !usedParts.Contains(p.Part)))
            //{
            //    var loc = $"{root}/{config.Part}".Replace("//", "/").TrimEnd('/');
            //    string lastmod;
            //    if (string.Equals(config.Lastmod, "date", StringComparison.OrdinalIgnoreCase))
            //        lastmod = DateTime.Now.ToString("yyyy-MM-dd");
            //    else
            //        lastmod = config.Lastmod;
            //    sitemapEntries.Add(new SitemapEntry
            //    {
            //        Loc = loc,
            //        Lastmod = FormatDate(lastmod),
            //        Changefreq = config.Changefreq,
            //        Priority = config.Priority
            //    });
            //}
            var ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urlset = new XElement(XName.Get("urlset", ns),
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                sitemapEntries.Select(e =>
                    new XElement(XName.Get("url", ns),
                        new XElement(XName.Get("loc", ns), e.Loc),
                        new XElement(XName.Get("lastmod", ns), e.Lastmod),
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
