using System;
using System.IO;
using System.Linq;
using msitemap.Models;
using System.Globalization;
using msitemap.Services;
using CommandLine;

namespace msitemap
{
    /// <summary>
    /// Точка входа консольного приложения для генерации sitemap.xml.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Основной метод запуска приложения.
        /// </summary>
        /// <param name="args">Аргументы командной строки: --xslt, --config, --output, --help, --version</param>
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunWithOptions)
                .WithNotParsed(errors =>
                {
                    // Если явно указан --help, CommandLineParser сам выведет справку
                    // Если явно указан --version, CommandLineParser сам выведет версию
                    // В остальных случаях — выводим краткую подсказку
                    if (!args.Contains("--help") && !args.Contains("--version"))
                    {
                        Console.WriteLine("Используйте --help для справки.");
                    }
                });
        }

        /// <summary>
        /// Основная логика генерации sitemap.xml с обработкой опций.
        /// </summary>
        static void RunWithOptions(Options opts)
        {
            // Обработка --version
            if (opts.Version)
            {
                Console.WriteLine("msitemap version 1.0.0");
                return;
            }
            // Обработка --help (CommandLineParser сам выводит справку, но можно добавить свою логику)
            if (opts.Help)
            {
                Console.WriteLine("Утилита для генерации sitemap.xml на основе XML, XSLT и config.json");
                Console.WriteLine("Пример использования:");
                Console.WriteLine("  msitemap --xslt transform.xslt --config config.json --output sitemap.xml");
                return;
            }

            // Получение путей из опций или значений по умолчанию
            string xsltFile = opts.XsltFile ?? (opts.ConfigFile == null && opts.SitemapFile == null && opts.Version == false && opts.Help == false && opts.XsltFile == null && opts.ConfigFile == null ? null : "transform.xslt");
            string configFile = opts.ConfigFile ?? "config.json";
            string sitemapFile = opts.SitemapFile ?? "sitemap.xml";

            if (string.IsNullOrEmpty(xsltFile))
            {
                Console.WriteLine("Ошибка: Необходимо указать XSLT-файл через --xslt.");
                return;
            }

            // Проверка наличия файлов XSLT и config.json
            if (!File.Exists(xsltFile))
            {
                Console.WriteLine($"Ошибка: XSLT-файл '{xsltFile}' не найден.");
                return;
            }
            if (!File.Exists(configFile))
            {
                Console.WriteLine($"Ошибка: config-файл '{configFile}' не найден.");
                return;
            }

            Console.WriteLine($"Используется XSLT: {xsltFile}");
            Console.WriteLine($"Используется config: {configFile}");
            Console.WriteLine($"Имя sitemap-файла: {sitemapFile}");

            // Поиск всех XML-файлов в текущем каталоге, кроме XSLT и config.json
            var currentDir = Directory.GetCurrentDirectory();
            var xmlFiles = Directory.GetFiles(currentDir, "*.xml", SearchOption.TopDirectoryOnly)
                .Where(f => !string.Equals(Path.GetFileName(f), Path.GetFileName(xsltFile), StringComparison.OrdinalIgnoreCase))
                .Where(f => !string.Equals(Path.GetFileName(f), Path.GetFileName(configFile), StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (xmlFiles.Count == 0)
            {
                Console.WriteLine("Не найдено ни одного XML-файла для обработки.");
                return;
            }

            Console.WriteLine($"Найдено XML-файлов: {xmlFiles.Count}");
            foreach (var file in xmlFiles)
            {
                Console.WriteLine($"  {Path.GetFileName(file)}");
            }

            // Обработка XML-файлов и сбор PageGroupItem через JsonList
            JsonList jsonList;
            try
            {
                jsonList = new JsonList(xsltFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки XSLT: {ex.Message}");
                return;
            }

            foreach (var xmlFile in xmlFiles)
            {
                try
                {
                    jsonList.AddXml(xmlFile);
                    Console.WriteLine($"Успешно обработан: {Path.GetFileName(xmlFile)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка обработки {Path.GetFileName(xmlFile)}: {ex.Message}");
                }
            }

            var allItems = jsonList.Items.ToList();
            if (allItems.Count == 0)
            {
                Console.WriteLine("Не удалось получить ни одной записи после объединения JSON.");
                return;
            }
            Console.WriteLine($"Всего записей после объединения: {allItems.Count}");

            // Генерация sitemap.xml через класс Sitemap
            try
            {
                var sitemap = new Sitemap(configFile, currentDir);
                int sitemapCount = sitemap.Make(jsonList, sitemapFile);
                Console.WriteLine($"Сформировано sitemap записей: {sitemapCount}");
                Console.WriteLine($"Файл {sitemapFile} успешно создан в каталоге: {currentDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка генерации sitemap.xml: {ex.Message}");
            }
        }
    }
}
