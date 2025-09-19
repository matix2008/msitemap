# msitemap

Генератор sitemap.xml на основе XML-файлов, XSLT-преобразования и конфигурационного файла config.json.

## Возможности
- Автоматический поиск XML-файлов в каталоге
- Преобразование XML в JSON с помощью XSLT
- Гибкая настройка структуры sitemap через config.json
- Строгая валидация конфигурации
- Удобная командная строка с поддержкой --help и --version

## Быстрый старт

1. **Соберите проект:**
   ```sh
   dotnet build
   ```

2. **Подготовьте файлы:**
   - XSLT-файл для преобразования XML в JSON
   - config.json (пример ниже)
   - XML-файлы для обработки

3. **Запустите генератор:**
   ```sh
   msitemap --xslt transform.xslt --config config.json --output sitemap.xml
   ```
   Параметры:
   - `--xslt` — путь к XSLT-файлу (обязательно)
   - `--config` — путь к config.json (по умолчанию config.json)
   - `--output` — имя выходного sitemap-файла (по умолчанию sitemap.xml)
   - `--help` — справка
   - `--version` — версия

## Пример config.json
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

## Требования к config.json
- Формат: массив объектов
- Все поля обязательны: `part`, `loc`, `lastmod`, `changefreq`, `priority`
- `part` — уникален
- `loc`, `lastmod` — только `slug`, `part`, `date` или дата (для lastmod)
- `changefreq` — только: always, hourly, daily, weekly, monthly, yearly, never
- `priority` — число от 0.0 до 1.0

## Пример XSLT
XSLT должен преобразовывать XML в JSON-структуру, совместимую с PageGroupItem.

## Лицензия
MIT
