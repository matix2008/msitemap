namespace msitemap.Models
{
    /// <summary>
    /// Элемент группы страниц, получаемый после XSLT-преобразования и парсинга JSON.
    /// </summary>
    public class PageGroupItem
    {
        /// <summary>
        /// Тип/группа страницы (например, "tariffs/view", "articles").
        /// </summary>
        public string Part { get; set; } = string.Empty;
        /// <summary>
        /// URL-часть или slug страницы.
        /// </summary>
        public string Slug { get; set; } = string.Empty;
        /// <summary>
        /// Дата последнего изменения или публикации.
        /// </summary>
        public string Date { get; set; } = string.Empty;
    }

    /// <summary>
    /// Конфигурация для генерации sitemap для определённого part.
    /// </summary>
    public class ConfigEntry
    {
        /// <summary>
        /// Тип/группа страницы (part), к которому применяется эта конфигурация.
        /// </summary>
        public string Part { get; set; } = string.Empty;
        /// <summary>
        /// Имя поля PageGroupItem, используемое для loc.
        /// </summary>
        public string Loc { get; set; } = string.Empty;
        /// <summary>
        /// Имя поля PageGroupItem или фиксированная дата для lastmod.
        /// </summary>
        public string Lastmod { get; set; } = string.Empty;
        /// <summary>
        /// Частота изменения страницы (например, monthly).
        /// </summary>
        public string Changefreq { get; set; } = string.Empty;
        /// <summary>
        /// Приоритет страницы в sitemap.
        /// </summary>
        public double Priority { get; set; }
    }

    /// <summary>
    /// Итоговая запись для sitemap.xml.
    /// </summary>
    public class SitemapEntry
    {
        /// <summary>
        /// URL страницы.
        /// </summary>
        public string Loc { get; set; } = string.Empty;
        /// <summary>
        /// Дата последнего изменения (ISO 8601).
        /// </summary>
        public string Lastmod { get; set; } = string.Empty;
        /// <summary>
        /// Частота изменения.
        /// </summary>
        public string Changefreq { get; set; } = string.Empty;
        /// <summary>
        /// Приоритет.
        /// </summary>
        public double Priority { get; set; }
    }
}
