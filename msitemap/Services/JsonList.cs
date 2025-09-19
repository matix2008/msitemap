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
    /// ����� ��� �������� � ����������� ������ �������, ���������� ����� XSLT-�������������� XML-������.
    /// </summary>
    public class JsonList
    {
        // XSLT-���������������
        private readonly XslCompiledTransform _xslt;
        // ������ ���� ��������� PageGroupItem, ��������� �� ���� XML
        private readonly List<PageGroupItem> _items = [];

        /// <summary>
        /// �������� ����������� ������ ���� ��������� PageGroupItem.
        /// </summary>
        public IReadOnlyList<PageGroupItem> Items => _items;

        /// <summary>
        /// �����������. ��������� � ����������� XSLT-����.
        /// </summary>
        /// <param name="xsltPath">���� � XSLT-�����</param>
        public JsonList(string xsltPath)
        {
            _xslt = new XslCompiledTransform();
            _xslt.Load(xsltPath);
        }

        /// <summary>
        /// ��������� ������ �� XML-�����, �������� � ���� XSLT-�������������� � ����� ��������� ��� JSON.
        /// </summary>
        /// <param name="xmlPath">���� � XML-�����</param>
        /// <exception cref="Exception">����� ������ �������������� ������</exception>
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
