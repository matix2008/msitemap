using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq; // �������� using ��� XElement � XDocument
using msitemap.Models;

namespace msitemap.Services
{
    /// <summary>
    /// ����� ��� ��������� sitemap.xml �� ������ ������ ������� � ������������.
    /// </summary>
    public class Sitemap
    {
        // ������ ������������ ��� ������ part
        private readonly List<ConfigEntry> _configEntries;
        // �������, � ������� ����� ������� sitemap.xml
        private readonly string _outputDirectory;

        /// <summary>
        /// �����������. ��������� � ������ config.json.
        /// </summary>
        /// <param name="configPath">���� � config.json</param>
        /// <param name="outputDirectory">������� ��� sitemap.xml (�� ��������� � �������)</param>
        /// <exception cref="FileNotFoundException">���� config.json �� ������</exception>
        /// <exception cref="Exception">���� config.json ���� ��� ���������</exception>
        public Sitemap(string configPath, string? outputDirectory = null)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Config-���� �� ������: {configPath}");
            var configText = File.ReadAllText(configPath);
            _configEntries = JsonSerializer.Deserialize<List<ConfigEntry>>(configText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
                ?? throw new Exception("Config-���� ���� ��� ���������");
            if (_configEntries.Count == 0)
                throw new Exception("Config-���� �� �������� �� ����� ������");
            // ������� ���������
            ConfigValidator.Validate(_configEntries);
            _outputDirectory = outputDirectory ?? Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// ���������� sitemap.xml �� ������ ������ ������� � ��������� ��� � ��������� ����.
        /// </summary>
        /// <param name="list">������ ������� (JsonList)</param>
        /// <param name="sitemapFileName">��� ����� sitemap (��������, sitemap.xml)</param>
        /// <returns>���������� ������� � sitemap</returns>
        public int Make(JsonList list, string sitemapFileName)
        {
            var sitemapEntries = new List<SitemapEntry>();
            // ��� ������ �������� ���� ��������������� config � ��������� SitemapEntry
            foreach (var item in list.Items)
            {
                var config = _configEntries.FirstOrDefault(c => c.Part == item.Part);
                if (config == null)
                {
                    // ���� ��� part ��� ������������ � ����������
                    continue;
                }
                var entry = new SitemapEntry
                {
                    // ��������� Loc ��� /part/{�������� �� config.Loc}
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
        /// �������� �������� �������� PageGroupItem �� ����� (������������ ��� ����������� loc, lastmod � ��.)
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
        /// ������������� ���� � ������� ISO 8601 (yyyy-MM-dd) ��� lastmod
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
