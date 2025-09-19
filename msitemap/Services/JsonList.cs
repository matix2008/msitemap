using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;
using System.Xml.Xsl;
using msitemap.Models;

namespace msitemap.Services
{
    /// <summary>
    /// Класс для хранения и объединения данных страниц, полученных после XSLT-преобразования XML-файлов.
    /// </summary>
    public class JsonList
    {
        // XSLT-преобразователь
        private readonly XslCompiledTransform _xslt;
        // Список всех элементов PageGroupItem, собранных из всех XML
        private readonly List<PageGroupItem> _items = [];

        /// <summary>
        /// Получить объединённый список всех элементов PageGroupItem.
        /// </summary>
        public IReadOnlyList<PageGroupItem> Items => _items;

        /// <summary>
        /// Конструктор. Загружает и компилирует XSLT-файл.
        /// </summary>
        /// <param name="xsltPath">Путь к XSLT-файлу</param>
        public JsonList(string xsltPath)
        {
            _xslt = new XslCompiledTransform();
            _xslt.Load(xsltPath);
        }

        /// <summary>
        /// Добавляет данные из XML-файла, применяя к нему XSLT-преобразование и парся результат как JSON.
        /// </summary>
        /// <param name="xmlPath">Путь к XML-файлу</param>
        /// <exception cref="Exception">Любые ошибки пробрасываются наружу</exception>
        public void AddXml(string xmlPath)
        {
            using var xmlReader = XmlReader.Create(xmlPath);
            using var sw = new StringWriter();
            using var xw = XmlWriter.Create(sw, _xslt.OutputSettings);
            _xslt.Transform(xmlReader, xw);
            string json = sw.ToString();
            var dict = JsonSerializer.Deserialize<Dictionary<string, List<PageGroupItem>>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            if (dict != null)
            {
                foreach (var list in dict.Values)
                    _items.AddRange(list);
            }
        }
    }
}
