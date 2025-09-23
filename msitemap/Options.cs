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

        [Option('s', "skip", Required = false, HelpText = "����� (���������) ��� �������� XML-������ �� �����.")]
        public string? SkipMask { get; set; }

        [Option('v', "version", HelpText = "�������� ������ ����������.")]
        public bool Version { get; set; }

        [Option('h', "help", HelpText = "�������� �������.")]
        public bool Help { get; set; }

        [Option('k', "check", HelpText = "��������� URLs �� ����� �����.")]
        public bool CheckSitemap { get; set; }
    }
}
