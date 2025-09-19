using CommandLine;

namespace msitemap
{
    /// <summary>
    /// ����� ��������� ������ ��� ���������� sitemap.
    /// </summary>
    public class Options
    {
        [Option('x', "xslt", Required = false, HelpText = "���� � XSLT-�����.")]
        public string? XsltFile { get; set; }

        [Option('c', "config", Required = false, HelpText = "���� � config.json.")]
        public string? ConfigFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "��� ��������� sitemap-�����.")]
        public string? SitemapFile { get; set; }

        [Option('d', "directory", Required = false, HelpText = "������� ���������� ��� ������ XML � ������ sitemap.")]
        public string? WorkingDirectory { get; set; }

        [Option("skip", Required = false, HelpText = "����� (���������) ��� �������� XML-������ �� �����.")]
        public string? SkipMask { get; set; }

        [Option("version", HelpText = "�������� ������ ����������.")]
        public bool Version { get; set; }

        [Option("help", HelpText = "�������� �������.")]
        public bool Help { get; set; }
    }
}
