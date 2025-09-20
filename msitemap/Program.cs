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
            // Установка рабочей директории, если указана
            if (!string.IsNullOrWhiteSpace(opts.WorkingDirectory))
            {
                if (!Directory.Exists(opts.WorkingDirectory))
                {
                    Logger.Log($"Ошибка: рабочая директория '{opts.WorkingDirectory}' не существует.");
                    return;
                }
                Directory.SetCurrentDirectory(opts.WorkingDirectory);
            }

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
                Console.WriteLine("  msitemap --xslt transform.xslt --config config.json --output sitemap.xml --directory ./data");
                return;
            }

            // Получение путей из опций или значений по умолчанию
            string xsltFile = opts.XsltFile ?? "transform.xslt";
            string configFile = opts.ConfigFile ?? "config.json";
            string sitemapFile = opts.SitemapFile ?? "sitemap.xml";

            // Проверка наличия файлов XSLT и config.json
            if (!File.Exists(xsltFile))
            {
                Logger.Log($"Ошибка: XSLT-файл '{xsltFile}' не найден.");
                return;
            }
            if (!File.Exists(configFile))
            {
                Logger.Log($"Ошибка: config-файл '{configFile}' не найден.");
                return;
            }

            Logger.Log($"Используется XSLT: {xsltFile}");
            Logger.Log($"Используется config: {configFile}");
            Logger.Log($"Имя sitemap-файла: {sitemapFile}");
            Logger.Log($"Рабочая директория: {Directory.GetCurrentDirectory()}");

            // Поиск всех XML-файлов в текущем каталоге, кроме XSLT и config.json
            var currentDir = Directory.GetCurrentDirectory();
            var xmlFiles = Directory.GetFiles(currentDir, "*.xml", SearchOption.TopDirectoryOnly)
                .Where(f => !string.Equals(Path.GetFileName(f), Path.GetFileName(xsltFile), StringComparison.OrdinalIgnoreCase))
                .Where(f => !string.Equals(Path.GetFileName(f), Path.GetFileName(configFile), StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Применяем маску пропуска, если указана
            if (!string.IsNullOrWhiteSpace(opts.SkipMask))
            {
                xmlFiles = xmlFiles
                    .Where(f => !Path.GetFileName(f).Contains(opts.SkipMask, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (xmlFiles.Count == 0)
            {
                Logger.Log("Не найдено ни одного XML-файла для обработки.");
                return;
            }

            Logger.Log($"Найдено XML-файлов: {xmlFiles.Count}");
            foreach (var file in xmlFiles)
            {
                Logger.Log($"  {Path.GetFileName(file)}");
            }

            // Обработка XML-файлов и сбор PageGroupItem через JsonList
            JsonList jsonList;
            try
            {
                jsonList = new JsonList(xsltFile);
            }
            catch (Exception ex)
            {
                Logger.Log($"Ошибка загрузки XSLT: {ex.Message}");
                return;
            }

            foreach (var xmlFile in xmlFiles)
            {
                try
                {
                    jsonList.AddXml(xmlFile);
                    Logger.Log($"Успешно обработан: {Path.GetFileName(xmlFile)}");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка обработки {Path.GetFileName(xmlFile)}: {ex.Message}");
                }
            }

            var allItems = jsonList.Items.ToList();
            if (allItems.Count == 0)
            {
                Logger.Log("Не удалось получить ни одной записи после объединения JSON.");
                return;
            }
            Logger.Log($"Всего записей после объединения: {allItems.Count}");

            // Генерация sitemap.xml через класс Sitemap
            try
            {
                var sitemap = new Sitemap(configFile, currentDir);
                int sitemapCount = sitemap.Make(jsonList, sitemapFile);
                Logger.Log($"Сформировано sitemap записей: {sitemapCount}");
                Logger.Log($"Файл {sitemapFile} успешно создан в каталоге: {currentDir}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Ошибка генерации sitemap.xml: {ex.Message}");
            }
        }
    }
}
