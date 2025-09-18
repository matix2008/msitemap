using CommandLine;

namespace msitemap
{
    /// <summary>
    /// Опции командной строки для генератора sitemap.
    /// </summary>
    public class Options
    {
        [Option('x', "xslt", Required = false, HelpText = "Путь к XSLT-файлу.")]
        public string? XsltFile { get; set; }

        [Option('c', "config", Required = false, HelpText = "Путь к config.json.")]
        public string? ConfigFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Имя выходного sitemap-файла.")]
        public string? SitemapFile { get; set; }

        [Option("version", HelpText = "Показать версию приложения.")]
        public bool Version { get; set; }

        [Option("help", HelpText = "Показать справку.")]
        public bool Help { get; set; }
    }
}
