# Структура кодовой базы

## Обзор структуры решения

Проект организован как .NET Solution с тремя основными проектами:

```
TestUmbraco.sln
├── TestUmbraco/                    # Основной веб-проект (Presentation Layer)
├── TestUmbraco.Application/        # Слой бизнес-логики (Application Layer)
└── TestUmbraco.Domain/             # Слой данных (Domain/Data Layer)
```

## TestUmbraco (Web Project)

Основной веб-проект с Umbraco CMS.

### Структура папок

```
TestUmbraco/
├── App_Plugins/                    # Кастомные плагины для бэк-офиса Umbraco
│   ├── MenuIdPicker/              # Плагин выбора ID меню
│   │   ├── menu-id-picker.js
│   │   ├── menu-id-picker.css
│   │   └── umbraco-package.json
│   ├── Vokseverk.ColorSelector/   # Плагин выбора цвета
│   │   ├── color-selector.js
│   │   └── umbraco-package.json
│   └── UniversalBackground/       # Плагин управления фонами (компилируется из TS)
│
├── Composers/                      # Umbraco Composers для регистрации сервисов
│   └── MediaCacheComposer.cs
│
├── Controllers/                    # API контроллеры
│   ├── EmailController.cs         # Отправка email
│   ├── MediaCacheController.cs    # Кэширование медиа
│   └── UniversalBackgroundController.cs # API для фоновых изображений
│
├── Helpers/                        # Вспомогательные классы
│   ├── HtmlHelper.cs              # Помощники для HTML
│   └── ImageHelper.cs             # Оптимизация и рендеринг изображений
│
├── Models/                         # Модели данных
│   ├── Configuration/             # Модели конфигурации
│   │   └── ImageOptimizationSettings.cs
│   ├── BackgroundClassesModel.cs
│   └── ImageModel.cs
│
├── Views/                          # Razor views (не показаны в дереве)
│
├── wwwroot/                        # Статические файлы
│   ├── App_Plugins/               # Скомпилированные плагины
│   ├── css/                       # CSS файлы
│   ├── js/                        # JavaScript файлы
│   └── media/                     # Загруженные медиа-файлы
│
├── Logs/                           # Логи приложения
│   └── log-YYYYMMDD.txt
│
├── appsettings.json                # Основная конфигурация
├── appsettings.Development.json    # Конфигурация для разработки
└── TestUmbraco.csproj              # Файл проекта
```

### Ключевые компоненты

#### Controllers
- **EmailController**: Обработка отправки email с формы сайта
- **MediaCacheController**: Управление кэшем медиа-файлов
- **UniversalBackgroundController**: API для получения CSS фоновых изображений

#### Helpers
- **ImageHelper**: 
  - Рендеринг responsive изображений с srcset
  - Поддержка WebP формата
  - Генерация picture элементов
  - Lazy loading
  - Placeholder для отсутствующих изображений

#### Models
- **ImageModel**: Модель изображения с метаданными
- **BackgroundClassesModel**: Модель для CSS классов фонов
- **ImageOptimizationSettings**: Настройки оптимизации изображений

#### Composers
- **MediaCacheComposer**: Регистрация сервисов кэширования медиа

## TestUmbraco.Application (Application Layer)

Слой бизнес-логики и сервисов.

### Структура папок

```
TestUmbraco.Application/
├── Contracts/                      # Интерфейсы сервисов
│   └── (интерфейсы сервисов)
│
├── DTO/                            # Data Transfer Objects
│   └── (DTO классы)
│
├── Services/                       # Реализация сервисов
│   └── (сервисы приложения)
│
├── Application.cs                  # Точка входа слоя приложения
└── TestUmbraco.Application.csproj  # Файл проекта
```

### Зависимости
- **TestUmbraco.Domain**: Ссылка на слой данных
- **MailKit/MimeKit**: Для отправки email
- **Microsoft.Extensions.Configuration**: Для работы с конфигурацией

### Назначение
- Бизнес-логика приложения
- Сервисы для работы с данными
- Обработка бизнес-правил
- Координация между контроллерами и репозиториями

## TestUmbraco.Domain (Domain/Data Layer)

Слой данных и доменной логики.

### Структура папок

```
TestUmbraco.Domain/
├── Contracts/                      # Интерфейсы репозиториев
│   └── (интерфейсы репозиториев)
│
├── Models/                         # Доменные модели (Entity)
│   └── (модели базы данных)
│
├── Repositories/                   # Реализация репозиториев
│   └── (репозитории для работы с БД)
│
├── Migrations/                     # EF Core миграции
│   └── (файлы миграций)
│
├── AppDbContext.cs                 # Entity Framework DbContext
├── Domain.cs                       # Точка входа слоя домена
└── TestUmbraco.Domain.csproj       # Файл проекта
```

### Зависимости
- **Entity Framework Core 10.0.0**: ORM для работы с БД
- **Microsoft.Data.SqlClient**: Провайдер SQL Server

### Назначение
- Доменные модели (Entity)
- Репозитории для доступа к данным
- DbContext для Entity Framework
- Миграции базы данных

## Frontend структура

### TypeScript плагины

```
plugins-src/                        # Исходники TypeScript плагинов
└── UniversalBackground/
    └── components/
        └── (TypeScript компоненты)
```

Компилируются в:
```
TestUmbraco/wwwroot/App_Plugins/UniversalBackground/components/
```

### Node.js зависимости

```
node_modules/                       # NPM пакеты (не коммитится)
package.json                        # NPM конфигурация
package-lock.json                   # Зафиксированные версии пакетов
tsconfig.json                       # TypeScript конфигурация
```

## Конфигурационные файлы

### Корневой уровень

```
.gitignore                          # Git ignore правила
.dockerignore                       # Docker ignore правила
docker-compose.yml                  # Docker Compose конфигурация
Dockerfile                          # Docker образ
TestUmbraco.sln                     # Visual Studio Solution
```

### Umbraco конфигурация

```
TestUmbraco/
├── appsettings.json                # Основная конфигурация
├── appsettings.Development.json    # Конфигурация для разработки
├── appsettings-schema.json         # JSON Schema для appsettings
├── appsettings-schema.Umbraco.Cms.json
├── appsettings-schema.usync.json
└── appsettings-schema.blockpreview.json
```

## Важные папки и файлы

### Папки, которые НЕ коммитятся в Git
- `bin/` - скомпилированные файлы
- `obj/` - промежуточные файлы сборки
- `node_modules/` - NPM пакеты
- `Logs/` - логи приложения
- `App_Data/` - данные Umbraco (кэш, индексы)
- `umbraco/models/` - автогенерированные модели

### Папки с важными данными
- `App_Plugins/` - кастомные плагины Umbraco
- `Views/` - Razor шаблоны
- `wwwroot/` - статические файлы (CSS, JS, изображения)
- `Migrations/` - миграции базы данных

## Паттерны организации кода

### Dependency Flow
```
TestUmbraco (Web)
    ↓ зависит от
TestUmbraco.Application (Business Logic)
    ↓ зависит от
TestUmbraco.Domain (Data Access)
```

### Naming Patterns
- **Controllers**: `{Feature}Controller.cs` (например, `EmailController.cs`)
- **Services**: `I{Feature}Service.cs` (интерфейс), `{Feature}Service.cs` (реализация)
- **Repositories**: `I{Entity}Repository.cs`, `{Entity}Repository.cs`
- **Models**: `{Entity}Model.cs` или `{Feature}Model.cs`
- **Helpers**: `{Feature}Helper.cs`

### Namespace Patterns
- `TestUmbraco.{Folder}` - для веб-проекта
- `TestUmbraco.Application.{Folder}` - для слоя приложения
- `TestUmbraco.Domain.{Folder}` - для слоя данных

## Расширения и плагины

### Установленные Umbraco пакеты
- **uSync**: Синхронизация контента через файлы
- **Block Preview**: Предпросмотр блоков в бэк-офисе
- **SEO Toolkit**: SEO инструменты
- **MenuIdPicker**: Кастомный property editor
- **ColorSelector**: Выбор цвета
- **UniversalBackground**: Управление фонами (кастомная разработка)

### Расположение плагинов
- Исходники: `TestUmbraco/App_Plugins/{PluginName}/`
- После публикации: `TestUmbraco/wwwroot/App_Plugins/{PluginName}/`
