using System;
using System.IO;
using System.Linq;
using msitemap.Models;
using System.Globalization;
using msitemap.Services;
using CommandLine;
using System.Threading.Tasks;

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
        public static async Task Main(string[] args)
        {
            //Parser.Default.ParseArguments<Options>(args)
            //    .WithParsed(async opts => await RunWithOptions(opts))
            //    .WithNotParsed(errors =>
            //    {
            //        // Если явно указан --help, CommandLineParser сам выведет справку
            //        // Если явно указан --version, CommandLineParser сам выведет версию
            //        // В остальных случаях — выводим краткую подсказку
            //        if (!args.Contains("--help") && !args.Contains("--version"))
            //        {
            //            Console.WriteLine("Используйте --help для справки.");
            //        }
            //    });

            var parsed = Parser.Default.ParseArguments<Options>(args);

            // Используем MapResult, чтобы гарантированно дождаться RunWithOptions
            await parsed.MapResult(
                async (Options opts) =>
                {
                    await RunWithOptions(opts);
                    return 0;
                },
                errs => Task.FromResult(1)
            );
        }

        /// <summary>
        /// Основная логика генерации sitemap.xml с обработкой опций.
        /// </summary>
        static async Task RunWithOptions(Options opts)
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

            string sitemapFile = opts.SitemapFile ?? "sitemap.xml";

            if (opts.CheckSitemap)
            {
                if (!File.Exists(sitemapFile))
                {
                    Logger.Log($"Ошибка: файл карты сайта '{sitemapFile}' не найден.");
                    return;
                }

                try
                { 
                    string[] urls = [..Sitemap.Load(sitemapFile).Select(e => e.Loc)];

                    Logger.Log($"Начало проверки карты сайта: {sitemapFile}. {urls.Length} ссылок");

                    var results = await LinkChecker.CheckAsync(urls, degreeOfParallelism: 8);

                    int nOK = 0;
                    int nBroken404 = 0;
                    int nRedirect = 0;

                    foreach (var r in results.OrderBy(x => x.Url))
                    {
                        if( r.IsRedirect ) nRedirect++;
                        if( r.IsBroken404 ) nBroken404++;
                        if (r.Status == System.Net.HttpStatusCode.OK) nOK++;

                        Logger.Log(
                            $"{r.Url}\n" +
                            $"  Status: {r.Status?.ToString() ?? "N/A"}  " +
                            (r.IsRedirect ? $"Redirect→ {r.RedirectLocation}" : "") +
                            (r.IsBroken404 ? "  [BROKEN 404]" : "") +
                            (r.Error is not null ? $"  Error: {r.Error}" : "") +
                            $"  ({r.ElapsedMs} ms)"
                        );
                    }

                    Logger.Log($"Проверка карты сайта: {sitemapFile} завершена.");
                    Logger.Log($"Редирект: {nRedirect}");
                    Logger.Log($"Битых ссылок: {nBroken404}");
                    Logger.Log($"ОК: {nOK}");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Ошибка проверки ссылок из карты сайта: {ex.Message}");
                }

                return;
            }

            // Получение путей из опций или значений по умолчанию
            string xsltFile = opts.XsltFile ?? "transform.xslt";
            string configFile = opts.ConfigFile ?? "config.json";

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
                xmlFiles = [..xmlFiles
                    .Where(f => !Path.GetFileName(f).Contains(opts.SkipMask, StringComparison.OrdinalIgnoreCase))];
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
