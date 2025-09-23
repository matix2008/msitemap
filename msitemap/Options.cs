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

        [Option('d', "directory", Required = false, HelpText = "Рабочая директория для поиска XML и вывода sitemap.")]
        public string? WorkingDirectory { get; set; }

        [Option('s', "skip", Required = false, HelpText = "Маска (подстрока) для пропуска XML-файлов по имени.")]
        public string? SkipMask { get; set; }

        [Option('v', "version", HelpText = "Показать версию приложения.")]
        public bool Version { get; set; }

        [Option('h', "help", HelpText = "Показать справку.")]
        public bool Help { get; set; }

        [Option('k', "check", HelpText = "Проверяет URLs из карты сайта.")]
        public bool CheckSitemap { get; set; }
    }
}
