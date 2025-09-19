# msitemap

��������� sitemap.xml �� ������ XML-������, XSLT-�������������� � ����������������� ����� config.json.

## �����������
- �������������� ����� XML-������ � ��������
- �������������� XML � JSON � ������� XSLT
- ������ ��������� ��������� sitemap ����� config.json
- ������� ��������� ������������
- ������� ��������� ������ � ���������� --help � --version

## ������� �����

1. **�������� ������:**
   ```sh
   dotnet build
   ```

2. **����������� �����:**
   - XSLT-���� ��� �������������� XML � JSON
   - config.json (������ ����)
   - XML-����� ��� ���������

3. **��������� ���������:**
   ```sh
   msitemap --xslt transform.xslt --config config.json --output sitemap.xml
   ```
   ���������:
   - `--xslt` � ���� � XSLT-����� (�����������)
   - `--config` � ���� � config.json (�� ��������� config.json)
   - `--output` � ��� ��������� sitemap-����� (�� ��������� sitemap.xml)
   - `--help` � �������
   - `--version` � ������

## ������ config.json
```json
[
  {
    "part": "articles",
    "loc": "slug",
    "lastmod": "date",
    "changefreq": "monthly",
    "priority": 1.0
  },
  {
    "part": "services",
    "loc": "slug",
    "lastmod": "date",
    "changefreq": "monthly",
    "priority": 1.0
  }
]
```

## ���������� � config.json
- ������: ������ ��������
- ��� ���� �����������: `part`, `loc`, `lastmod`, `changefreq`, `priority`
- `part` � ��������
- `loc`, `lastmod` � ������ `slug`, `part`, `date` ��� ���� (��� lastmod)
- `changefreq` � ������: always, hourly, daily, weekly, monthly, yearly, never
- `priority` � ����� �� 0.0 �� 1.0

## ������ XSLT
XSLT ������ ��������������� XML � JSON-���������, ����������� � PageGroupItem.

## ��������
MIT
