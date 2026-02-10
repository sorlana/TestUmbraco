# Особенности и паттерны проекта

## Архитектурные паттерны

### Clean Architecture / Layered Architecture
Проект следует принципам чистой архитектуры с разделением на слои:

1. **Presentation Layer** (TestUmbraco)
   - Controllers для API endpoints
   - Views для рендеринга HTML
   - Helpers для вспомогательной логики представления

2. **Application Layer** (TestUmbraco.Application)
   - Бизнес-логика
   - Сервисы приложения
   - DTO для передачи данных

3. **Domain Layer** (TestUmbraco.Domain)
   - Доменные модели (Entities)
   - Репозитории для доступа к данным
   - DbContext

### Dependency Injection
Проект активно использует DI через встроенный контейнер ASP.NET Core:
- Регистрация сервисов через Umbraco Composers
- Constructor injection для всех зависимостей
- Использование интерфейсов для абстракции

### Repository Pattern
Доступ к данным организован через репозитории:
- Интерфейсы в `TestUmbraco.Domain/Contracts/`
- Реализации в `TestUmbraco.Domain/Repositories/`
- Абстракция от конкретной реализации доступа к данным

## Umbraco-специфичные паттерны

### Composers для регистрации сервисов
Вместо стандартного `Startup.cs` используются Umbraco Composers:

```csharp
public class MediaCacheComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IMediaCacheService, MediaCacheService>();
    }
}
```

### Models Builder - SourceCodeAuto
- Модели контента генерируются автоматически
- Namespace: `Umbraco.Cms.Web.Common.PublishedModels`
- Генерируются при изменении Document Types в бэк-офисе

### Property Editors
Кастомные property editors создаются как плагины:
- JavaScript/TypeScript компоненты
- Регистрация через `umbraco-package.json`
- Размещение в `App_Plugins/`

## Паттерны оптимизации изображений

### Responsive Images Pattern
Проект использует современный подход к responsive изображениям:

1. **Picture Element с Source Sets**:
```html
<picture>
  <source type="image/webp" srcset="..." sizes="...">
  <source srcset="..." sizes="...">
  <img src="..." alt="...">
</picture>
```

2. **Автоматическая генерация размеров**:
   - Определенные ширины: [400, 600, 800, 1000, 1200, 1920]
   - Генерация srcset для каждого размера
   - Автоматический расчет sizes на основе Bootstrap grid

3. **WebP с Fallback**:
   - Первый source для WebP
   - Второй source для JPEG/PNG
   - img тег как fallback для старых браузеров

### Lazy Loading
- Использование нативного `loading="lazy"`
- Опциональное отключение через параметр `lazyLoad`

### Caching Strategy
- Кэширование сгенерированного HTML изображений
- Кэширование CSS для фоновых изображений
- Использование MediaCacheService

## Конфигурационные паттерны

### Options Pattern
Настройки приложения типизированы через Options pattern:

```csharp
public class ImageOptimizationSettings
{
    public int[] DefaultWidths { get; set; }
    public int WebpQuality { get; set; }
    public int JpegQuality { get; set; }
    public bool EnableWebp { get; set; }
}
```

Регистрация:
```csharp
services.Configure<ImageOptimizationSettings>(
    configuration.GetSection("ImageOptimization"));
```

Использование:
```csharp
public ImageHelper(IOptions<ImageOptimizationSettings> settings)
{
    _settings = settings.Value;
}
```

### Hierarchical Configuration
- `appsettings.json` - базовая конфигурация
- `appsettings.Development.json` - переопределения для разработки
- Environment variables - переопределения для production

## Паттерны работы с данными

### Entity Framework Core
- Code-First подход
- Миграции для версионирования схемы БД
- DbContext для управления сессией

### Async/Await везде
Все операции ввода-вывода асинхронные:
- Database queries: `await context.SaveChangesAsync()`
- File operations: `await File.ReadAllTextAsync()`
- HTTP requests: `await httpClient.GetAsync()`

## Frontend паттерны

### Lit Web Components
Для кастомных элементов бэк-офиса используется Lit:
- Декларативные компоненты
- Reactive properties
- Shadow DOM

### TypeScript для типобезопасности
- Строгий режим TypeScript
- Типизация всех компонентов
- Экспериментальные декораторы

## Логирование

### Structured Logging с Serilog
- Логирование в консоль с цветами
- Логирование в файлы с ротацией
- Опциональное логирование в Event Log
- Структурированные логи для лучшего анализа

### Log Levels
- **Information**: Обычные операции
- **Warning**: Потенциальные проблемы
- **Error**: Ошибки, требующие внимания
- **Debug**: Детальная информация для отладки

## Безопасность

### reCAPTCHA Integration
- Защита форм от спама
- Конфигурация через appsettings.json
- Валидация на сервере

### SQL Injection Protection
- Использование параметризованных запросов через EF Core
- Никаких сырых SQL строк с конкатенацией

### XSS Protection
- Автоматическое экранирование в Razor
- Использование `IHtmlContent` для безопасного HTML

## Docker паттерны

### Multi-stage Build
Dockerfile использует multi-stage build:
1. **Build stage**: Сборка приложения с SDK
2. **Runtime stage**: Только runtime без SDK (меньший размер)

### Volume Mounting
- Медиа-файлы монтируются как volume
- Данные сохраняются между перезапусками контейнера

## Особенности разработки

### Concurrent Development
`npm run dev` запускает одновременно:
- TypeScript compiler в watch режиме
- ASP.NET Core приложение
- Использование `concurrently` пакета

### Hot Reload
- TypeScript компилируется автоматически при изменениях
- ASP.NET Core поддерживает hot reload для некоторых изменений
- Razor views перекомпилируются на лету в Development режиме

## Интеграции

### uSync для синхронизации
- Экспорт/импорт Document Types, Data Types, Templates
- Файловая синхронизация для работы в команде
- Автоматическая синхронизация при старте (опционально)

### Block Preview
- Предпросмотр Block Grid и Block List в бэк-офисе
- Подключение стилей и скриптов для preview
- Настройка через appsettings.json

### SEO Toolkit
- Мета-поля для SEO
- Генерация sitemap
- Аудит сайта
- Script manager

## Рекомендации по расширению

### Добавление нового контроллера
1. Создать в `TestUmbraco/Controllers/`
2. Наследовать от `ControllerBase` или `Controller`
3. Использовать атрибуты `[Route]` и `[ApiController]`
4. Инжектить зависимости через конструктор

### Добавление нового сервиса
1. Создать интерфейс в `TestUmbraco.Application/Contracts/`
2. Создать реализацию в `TestUmbraco.Application/Services/`
3. Зарегистрировать через Composer
4. Инжектить в контроллеры/другие сервисы

### Добавление нового плагина
1. Создать папку в `TestUmbraco/App_Plugins/{PluginName}/`
2. Добавить `umbraco-package.json`
3. Создать JavaScript/TypeScript файлы
4. Настроить копирование в wwwroot через .csproj

### Добавление миграции
1. Изменить модели в `TestUmbraco.Domain/Models/`
2. Запустить: `dotnet ef migrations add {Name} --startup-project ../TestUmbraco`
3. Проверить сгенерированную миграцию
4. Применить: `dotnet ef database update --startup-project ../TestUmbraco`

## Известные особенности

### .NET 10.0
Проект использует .NET 10.0 (preview/RC версия):
- Может требовать специфичных SDK
- Некоторые пакеты могут быть в preview
- Комментарий в .csproj: "ВЕРНИТЕ ОБРАТНО net10.0 - это правильно!"

### Razor Compilation
- `RazorCompileOnBuild` и `RazorCompileOnPublish` включены
- Необходимо для работы SourceCodeAuto Models Builder
- Razor файлы копируются в publish директорию

### App_Plugins Copying
Специальная настройка в .csproj для гарантированного копирования:
```xml
<Content Include="wwwroot\App_Plugins\**" 
         CopyToOutputDirectory="PreserveNewest" 
         CopyToPublishDirectory="PreserveNewest" />
```

### Nullable Reference Types
- Включены в TestUmbraco и TestUmbraco.Application
- В TestUmbraco.Domain используется `annotations` режим
- CS8618 warning отключен в Domain проекте
