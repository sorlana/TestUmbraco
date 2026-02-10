# Стиль кода и соглашения

## Общие принципы

### Язык кода
- **C# код**: Английские названия классов, методов, переменных
- **Комментарии**: Русский язык для XML-документации и inline-комментариев
- **UI тексты**: Русский язык для пользовательских сообщений

## C# Code Style

### Naming Conventions
- **Классы и интерфейсы**: PascalCase (например, `ImageHelper`, `IMediaCacheService`)
- **Методы**: PascalCase (например, `RenderMediaAsync`, `GetCachedImageHtmlAsync`)
- **Приватные поля**: camelCase с префиксом `_` (например, `_mediaCacheService`, `_logger`)
- **Параметры и локальные переменные**: camelCase (например, `cropAlias`, `lazyLoad`)
- **Константы**: PascalCase или UPPER_CASE
- **Свойства**: PascalCase (например, `Url`, `AltText`)

### Nullable Reference Types
- Проект использует `<Nullable>enable</Nullable>` в TestUmbraco и TestUmbraco.Application
- В TestUmbraco.Domain используется `<Nullable>annotations</Nullable>`
- Используйте `?` для nullable типов: `string?`, `IPublishedContent?`
- Проверяйте null перед использованием: `if (media == null) return ...`

### Async/Await
- Все методы с асинхронными операциями должны иметь суффикс `Async`
- Используйте `Task<T>` для возвращаемых значений
- Всегда используйте `await` вместо `.Result` или `.Wait()`
- Пример: `public async Task<IHtmlContent> RenderMediaAsync(...)`

### Dependency Injection
- Используйте constructor injection для зависимостей
- Регистрируйте сервисы через Composers в Umbraco
- Пример:
```csharp
public class UniversalBackgroundController : ControllerBase
{
    private readonly IMediaCacheService _mediaCacheService;
    private readonly ILogger<UniversalBackgroundController> _logger;

    public UniversalBackgroundController(
        IMediaCacheService mediaCacheService,
        ILogger<UniversalBackgroundController> logger)
    {
        _mediaCacheService = mediaCacheService;
        _logger = logger;
    }
}
```

### XML Documentation
- Используйте XML-комментарии для публичных методов и классов
- Комментарии на русском языке
- Пример:
```csharp
/// <summary>
/// Рендерит адаптивное изображение с поддержкой srcset и picture
/// </summary>
public async Task<IHtmlContent> RenderResponsiveMediaAsync(...)
```

### Error Handling
- Используйте try-catch для обработки ошибок
- Логируйте ошибки через ILogger
- Возвращайте fallback значения при ошибках (например, пустой HtmlString)
- Пример:
```csharp
try
{
    var result = await _mediaCacheService.GetCachedImageHtmlAsync(media, cropAlias, attributes);
    return result ?? new HtmlString(string.Empty);
}
catch (Exception)
{
    return new HtmlString(string.Empty);
}
```

### LINQ и Collections
- Используйте LINQ для работы с коллекциями
- Предпочитайте `List<T>` для изменяемых коллекций
- Используйте `Dictionary<TKey, TValue>` для key-value пар
- Проверяйте `.Any()` перед использованием коллекций

## TypeScript Code Style

### Naming Conventions
- **Классы**: PascalCase
- **Интерфейсы**: PascalCase с префиксом `I` (опционально)
- **Методы и функции**: camelCase
- **Переменные**: camelCase
- **Константы**: UPPER_CASE или camelCase

### TypeScript Configuration
- **Target**: ES2022
- **Module**: ES2022
- **Strict mode**: Включен
- **Decorators**: Экспериментальные декораторы включены

### Lit Components
- Используйте декораторы `@customElement`, `@property`, `@state`
- Следуйте паттернам Lit для веб-компонентов
- Используйте TypeScript для типизации

## Project Structure Conventions

### Организация файлов
- **Controllers**: API-контроллеры в `TestUmbraco/Controllers/`
- **Services**: Бизнес-логика в `TestUmbraco.Application/Services/`
- **Models**: Модели данных в соответствующих папках `Models/`
- **Helpers**: Вспомогательные классы в `TestUmbraco/Helpers/`
- **Repositories**: Репозитории в `TestUmbraco.Domain/Repositories/`

### Namespace Conventions
- Namespace должен соответствовать структуре папок
- Примеры:
  - `TestUmbraco.Controllers`
  - `TestUmbraco.Helpers`
  - `TestUmbraco.Application.Services`
  - `TestUmbraco.Domain.Models`

## Configuration

### appsettings.json
- Используйте строгую типизацию через Options pattern
- Создавайте классы настроек в `Models/Configuration/`
- Регистрируйте через `services.Configure<T>(configuration.GetSection("..."))`
- Пример: `ImageOptimizationSettings`

### Connection Strings
- Храните строки подключения в `ConnectionStrings` секции
- Используйте `umbracoDbDSN` для Umbraco
- Указывайте провайдер через `_ProviderName` суффикс

## Umbraco Specific

### Models Builder
- Режим: `SourceCodeAuto`
- Namespace: `Umbraco.Cms.Web.Common.PublishedModels`
- Модели генерируются автоматически

### Composers
- Используйте IComposer для регистрации сервисов
- Размещайте в папке `Composers/`
- Пример: `MediaCacheComposer`

### Controllers
- API контроллеры наследуются от `ControllerBase`
- Используйте атрибуты `[Route]` и `[ApiController]`
- Используйте `[HttpGet]`, `[HttpPost]` и т.д. для методов

## Git Conventions

### .gitignore
- Исключены: `bin/`, `obj/`, `node_modules/`, `Logs/`
- Исключены файлы Umbraco: `umbraco/models/`, `App_Data/`
- Исключены IDE файлы: `.vs/`, `.vscode/`
