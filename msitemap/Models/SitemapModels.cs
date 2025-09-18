namespace msitemap.Models
{
    /// <summary>
    /// ������� ������ �������, ���������� ����� XSLT-�������������� � �������� JSON.
    /// </summary>
    public class PageGroupItem
    {
        /// <summary>
        /// ���/������ �������� (��������, "tariffs/view", "articles").
        /// </summary>
        public string Part { get; set; } = string.Empty;
        /// <summary>
        /// URL-����� ��� slug ��������.
        /// </summary>
        public string Slug { get; set; } = string.Empty;
        /// <summary>
        /// ���� ���������� ��������� ��� ����������.
        /// </summary>
        public string Date { get; set; } = string.Empty;
    }

    /// <summary>
    /// ������������ ��� ��������� sitemap ��� ������������ part.
    /// </summary>
    public class ConfigEntry
    {
        /// <summary>
        /// ���/������ �������� (part), � �������� ����������� ��� ������������.
        /// </summary>
        public string Part { get; set; } = string.Empty;
        /// <summary>
        /// ��� ���� PageGroupItem, ������������ ��� loc.
        /// </summary>
        public string Loc { get; set; } = string.Empty;
        /// <summary>
        /// ��� ���� PageGroupItem ��� ������������� ���� ��� lastmod.
        /// </summary>
        public string Lastmod { get; set; } = string.Empty;
        /// <summary>
        /// ������� ��������� �������� (��������, monthly).
        /// </summary>
        public string Changefreq { get; set; } = string.Empty;
        /// <summary>
        /// ��������� �������� � sitemap.
        /// </summary>
        public double Priority { get; set; }
    }

    /// <summary>
    /// �������� ������ ��� sitemap.xml.
    /// </summary>
    public class SitemapEntry
    {
        /// <summary>
        /// URL ��������.
        /// </summary>
        public string Loc { get; set; } = string.Empty;
        /// <summary>
        /// ���� ���������� ��������� (ISO 8601).
        /// </summary>
        public string Lastmod { get; set; } = string.Empty;
        /// <summary>
        /// ������� ���������.
        /// </summary>
        public string Changefreq { get; set; } = string.Empty;
        /// <summary>
        /// ���������.
        /// </summary>
        public double Priority { get; set; }
    }
}
