# Архитектура системы TestUmbraco

## 1. Обзор архитектуры

### 1.1 Общая архитектура

TestUmbraco построен на основе многослойной (layered) архитектуры с четким разделением ответственности:

```
┌─────────────────────────────────────────────────────────┐
│           Presentation Layer (TestUmbraco)              │
│  Controllers, Views, Helpers, App_Plugins, wwwroot      │
└─────────────────────────────────────────────────────────┘
                          ↓ зависит от
┌─────────────────────────────────────────────────────────┐
│      Application Layer (TestUmbraco.Application)        │
│        Services, Contracts, DTO, Business Logic         │
└─────────────────────────────────────────────────────────┘
                          ↓ зависит от
┌─────────────────────────────────────────────────────────┐
│         Domain Layer (TestUmbraco.Domain)               │
│    Models, Repositories, DbContext, Migrations          │
└─────────────────────────────────────────────────────────┘
                          ↓ использует
┌─────────────────────────────────────────────────────────┐
│              Database (SQL Server)                      │
│           Umbraco Tables + Custom Tables                │
└─────────────────────────────────────────────────────────┘
```

### 1.2 Принципы архитектуры

- **Separation of Concerns**: Каждый слой имеет четко определенную ответственность
- **Dependency Inversion**: Зависимости направлены от верхних слоев к нижним
- **Dependency Injection**: Все зависимости инжектируются через конструкторы
- **Repository Pattern**: Абстракция доступа к данным
- **Service Layer**: Бизнес-логика изолирована в сервисах
- **Clean Architecture**: Независимость от фреймворков и UI

## 2. Слой представления (Presentation Layer)

### 2.1 Структура проекта TestUmbraco

```
TestUmbraco/
├── Controllers/          # API и Surface контроллеры
├── Views/               # Razor шаблоны
├── Helpers/             # Вспомогательные классы для представления
├── Models/              # View Models и конфигурация
├── Composers/           # Umbraco Composers для DI
├── App_Plugins/         # Кастомные плагины бэк-офиса
├── wwwroot/             # Статические файлы
└── appsettings.json     # Конфигурация приложения
```

### 2.2 Controllers

#### 2.2.1 UniversalBackgroundController

**Назначение**: API для получения CSS фоновых изображений

**Маршрут**: `/api/background/multiple`

**Зависимости**:
- `IMediaCacheService`: Кэширование и генерация CSS
- `ILogger<UniversalBackgroundController>`: Логирование

**Методы**:
- `GetMultipleCss(string backgrounds)`: Возвращает CSS для нескольких фонов

**Особенности**:
- Возвращает Content-Type: text/css
- Кэширует результат для повторных запросов
- Обрабатывает пустые параметры

#### 2.2.2 EmailController

**Назначение**: Обработка форм обратной связи

**Тип**: SurfaceController (Umbraco)

**Зависимости**:
- `IEmailService`: Отправка email
- `INotyfService`: Toast уведомления
- Стандартные зависимости Umbraco

**Методы**:
- `SendEmailRequest(EmailRequestDto)`: Отправка email-заявки
- `SendCallRequest(CallRequestDto)`: Отправка запроса звонка

**Особенности**:
- Использует reCAPTCHA валидацию (минимальный score 0.5)
- Отображает toast уведомления об успехе/ошибке
- Перенаправляет на текущую страницу после отправки

#### 2.2.3 MediaCacheController

**Назначение**: Управление кэшем медиа-файлов

**Маршрут**: `/umbraco/backoffice/api/media-cache`

**Авторизация**: Требует политику "BackOffice"

**Зависимости**:
- `IMediaCacheService`: Управление кэшем
- `ILoggingService`: Логирование

**Методы**:
- `GetCacheInfo()`: Информация о кэше (память, timestamp)
- `ClearCacheForMedia(Guid mediaKey)`: Очистка кэша для конкретного медиа
- `ClearAllMediaCache()`: Очистка всего кэша

### 2.3 Helpers

#### 2.3.1 ImageHelper

**Назначение**: Генерация адаптивных изображений с оптимизацией

**Зависимости**:
- `IMediaCacheService`: Кэширование HTML изображений
- `IOptions<ImageOptimizationSettings>`: Настройки оптимизации

**Ключевые методы**:

1. `RenderResponsiveMediaAsync()`: Основной метод генерации responsive изображений
   - Параметры: media, cropAlias, cssClass, altText, lazyLoad, attributes, responsiveWidths, sizes, gridColumns, includeWebP, skipWrapper
   - Возвращает: IHtmlContent с элементом `<picture>`
   - Генерирует srcset для разных размеров
   - Поддерживает WebP с fallback на JPEG/PNG
   - Автоматически рассчитывает sizes на основе Bootstrap grid

2. `GenerateImageUrl()`: Генерация URL с параметрами
   - Добавляет width, crop, format, quality
   - Использует настройки качества из конфигурации

3. `GenerateSrcSet()`: Генерация строки srcset
   - Создает URL для каждого размера
   - Формат: "url 400w, url 600w, ..."

4. `GenerateSizesFromGrid()`: Генерация sizes из Bootstrap классов
   - Парсит col-{n}, col-md-{n} и т.д.
   - Генерирует media queries для брейкпоинтов
   - Брейкпоинты: 1920, 1400, 1200, 992, 768, 576

5. `RenderPlaceholderAsync()`: Placeholder при отсутствии изображения
   - SVG иконка + текст "Нет изображения"

6. `RenderFallbackAsync()`: Fallback при ошибке
   - Простой img тег с базовым URL

**Алгоритм генерации responsive изображения**:

```
1. Проверка наличия media
   ├─ Если null → RenderPlaceholderAsync()
   └─ Если есть → продолжить

2. Определение размеров
   ├─ Использовать responsiveWidths или DefaultWidths из настроек
   └─ По умолчанию: [400, 600, 800, 1000, 1200, 1920]

3. Генерация sizes
   ├─ Если sizes передан → использовать его
   └─ Иначе → GenerateSizesFromGrid(gridColumns)

4. Создание элемента <picture>
   ├─ Если includeWebP и EnableWebp
   │  └─ Добавить <source type="image/webp" srcset="..." sizes="...">
   ├─ Добавить <source srcset="..." sizes="..."> (JPEG/PNG fallback)
   └─ Добавить <img src="..." srcset="..." sizes="..." alt="..." loading="lazy">

5. Обертка
   ├─ Если skipWrapper=false → обернуть в <div class="image-wrapper">
   └─ Иначе → вернуть только <picture>

6. Обработка ошибок
   └─ При исключении → RenderFallbackAsync()
```

#### 2.3.2 HtmlHelper (BackgroundHtmlExtensions)

**Назначение**: Extension методы для работы с фонами в Razor

**Методы**:
- `GetBackgroundClasses()`: Получение CSS классов для фона
- `AddBackgroundClasses()`: Объединение базовых и фоновых классов

### 2.4 Models

#### 2.4.1 ImageModel

**Назначение**: Модель изображения с метаданными

**Свойства**:
- `Url`: URL изображения
- `AltText`: Альтернативный текст
- `Title`: Заголовок
- `Width`, `Height`: Размеры
- `FocalPoint`: Точка фокуса для кропа
- `Attributes`: Дополнительные HTML атрибуты
- `CropAlias`: Алиас кропа
- `LazyLoad`: Флаг lazy loading (по умолчанию true)

#### 2.4.2 BackgroundClassesModel

**Назначение**: Модель для CSS классов фоновых изображений

**Свойства**:
- `Settings`: Настройки из Umbraco элемента
- `ComponentId`: Уникальный ID компонента
- `Prefix`: Префикс для CSS классов
- `BaseClass`, `AdditionalClasses`: CSS классы
- `Tag`: HTML тег (по умолчанию "div")
- `ContainerClass`, `ElementId`: Атрибуты контейнера
- `Content`: Функция рендеринга контента
- `RawContent`: Сырой HTML контент
- `Placeholder`: Placeholder текст

#### 2.4.3 ImageOptimizationSettings

**Назначение**: Конфигурация оптимизации изображений

**Свойства**:
- `DefaultWidths`: Размеры для srcset (по умолчанию [400, 600, 800, 1000, 1200, 1920])
- `WebpQuality`: Качество WebP (по умолчанию 80)
- `JpegQuality`: Качество JPEG (по умолчанию 85)
- `EnableWebp`: Включить WebP (по умолчанию true)
- `EnableLazyLoading`: Включить lazy loading (по умолчанию true)

### 2.5 Composers

#### 2.5.1 MediaCacheComposer

**Назначение**: Регистрация сервисов кэширования в DI контейнере

**Метод**: `Compose(IUmbracoBuilder builder)`

**Регистрируемые сервисы**:
- `IMediaCacheService` → `MediaCacheService` (Singleton)
- Другие сервисы кэширования

### 2.6 App_Plugins

#### 2.6.1 MenuIdPicker

**Файлы**:
- `menu-id-picker.js`: JavaScript компонент
- `menu-id-picker.css`: Стили
- `umbraco-package.json`: Манифест плагина

**Назначение**: Property editor для выбора ID меню

#### 2.6.2 Vokseverk.ColorSelector

**Файлы**:
- `color-selector.js`: JavaScript компонент
- `umbraco-package.json`: Манифест плагина

**Назначение**: Property editor для выбора цвета

#### 2.6.3 UniversalBackground

**Исходники**: `plugins-src/UniversalBackground/` (TypeScript)

**Компилируется в**: `wwwroot/App_Plugins/UniversalBackground/`

**Технологии**:
- TypeScript 5.9.3
- Lit 3.3.2 (Web Components)
- @umbraco-cms/backoffice 16.5.0

**Назначение**: Управление фоновыми изображениями в Grid редакторе

## 3. Слой приложения (Application Layer)

### 3.1 Структура проекта TestUmbraco.Application

```
TestUmbraco.Application/
├── Contracts/           # Интерфейсы сервисов
├── Services/            # Реализация сервисов
├── DTO/                 # Data Transfer Objects
└── Application.cs       # Точка входа слоя
```

### 3.2 Contracts (Интерфейсы)

#### 3.2.1 IEmailService

**Назначение**: Отправка email с форм обратной связи

**Методы**:
- `Task SendEmailRequestAsync(EmailRequestDto emailRequest)`: Отправка email-заявки
- `Task SendCallRequestAsync(CallRequestDto callRequest)`: Отправка запроса звонка

**Реализация**: Использует MailKit/MimeKit для отправки через SMTP

**Конфигурация** (из appsettings.json):
```json
{
  "Email": {
    "EmailSenderAddress": "me_site@max-log.ru",
    "EmailSenderName": "Почта с сайта e-max.tech",
    "EmailSenderPassword": "***",
    "EmailReceiverAddress": "gorbunovaav@e-max.tech",
    "EmailReceiverName": "Администрация сайта",
    "EmailSubject": "Заявка с сайта e-max.tech",
    "SmtpServer": "mail.nic.ru",
    "SmtpPort": 465,
    "UseSsl": true
  }
}
```

### 3.3 DTO (Data Transfer Objects)

#### 3.3.1 EmailRequestDto

**Назначение**: DTO для email-заявки

**Предполагаемые поля**:
- Name: Имя отправителя
- Email: Email отправителя
- Subject: Тема сообщения
- Message: Текст сообщения
- Phone (опционально): Телефон

#### 3.3.2 CallRequestDto

**Назначение**: DTO для запроса звонка

**Предполагаемые поля**:
- Name: Имя
- Phone: Телефон
- PreferredTime (опционально): Предпочтительное время звонка

### 3.4 Services

Сервисы реализуют бизнес-логику приложения:
- Валидация данных
- Обработка бизнес-правил
- Координация между контроллерами и репозиториями
- Трансформация данных между слоями

## 4. Слой домена (Domain Layer)

### 4.1 Структура проекта TestUmbraco.Domain

```
TestUmbraco.Domain/
├── Contracts/           # Интерфейсы репозиториев
├── Models/              # Доменные модели (Entity)
├── Repositories/        # Реализация репозиториев
├── Migrations/          # EF Core миграции
├── AppDbContext.cs      # DbContext
└── Domain.cs            # Точка входа слоя
```

### 4.2 AppDbContext

**Назначение**: Entity Framework DbContext для работы с БД

**Провайдер**: Microsoft.Data.SqlClient

**Строка подключения**:
```
Server=localhost,1433;
Database=TestUmbraco;
User Id=sa;
Password=***;
TrustServerCertificate=true;
MultipleActiveResultSets=true
```

**Особенности**:
- Использует SQL Server
- Поддерживает множественные активные наборы результатов (MARS)
- TrustServerCertificate для локальной разработки

### 4.3 Repositories

**Паттерн**: Repository Pattern

**Назначение**: Абстракция доступа к данным

**Структура**:
- Интерфейс в `Contracts/I{Entity}Repository.cs`
- Реализация в `Repositories/{Entity}Repository.cs`

**Преимущества**:
- Изоляция логики доступа к данным
- Упрощение тестирования (mock репозиториев)
- Единая точка для запросов к БД

### 4.4 Migrations

**Инструмент**: Entity Framework Core Migrations

**Команды**:
- Создание: `dotnet ef migrations add {Name} --startup-project ../TestUmbraco`
- Применение: `dotnet ef database update --startup-project ../TestUmbraco`
- Откат: `dotnet ef database update {PreviousMigration} --startup-project ../TestUmbraco`

## 5. Сервисы (Services)

### 5.1 IMediaCacheService

**Назначение**: Кэширование и оптимизация медиа-контента

**Методы**:

1. **Получение URL**:
   - `GetCachedMediaUrlAsync(Guid mediaKey, ...)`: По GUID
   - `GetCachedMediaUrlAsync(IPublishedContent media, ...)`: По IPublishedContent
   - Параметры: cropAlias, width, height

2. **Генерация HTML**:
   - `GetCachedImageHtmlAsync(Guid mediaKey, ...)`: HTML по GUID
   - `GetCachedImageHtmlAsync(IPublishedContent media, ...)`: HTML по IPublishedContent
   - Возвращает готовый HTML с кэшированием

3. **Фоновые изображения**:
   - `GetCachedBackgroundCssAsync(string backgrounds)`: CSS для нескольких фонов
   - `GenerateBackgroundCssAsync(Guid mediaGuid, ...)`: CSS для одного фона
   - Параметры: className, minHeight, size, position

4. **Управление кэшем**:
   - `ClearCacheForMedia(Guid mediaKey)`: Очистка для конкретного медиа
   - `ClearAllCache()`: Полная очистка кэша

5. **Оптимизация**:
   - `GeneratePictureElement(...)`: Генерация элемента picture
   - `ConvertToWebP(string url)`: Конвертация в WebP
   - `GenerateSrcSet(...)`: Генерация srcset

**Стратегия кэширования**:
- In-memory кэш для HTML и CSS
- Кэш изображений на диске (ImageSharp)
- TTL: 365 дней для изображений, 7 дней для браузера

### 5.2 IUmbracoBackgroundService

**Назначение**: Обработка фоновых изображений из Umbraco элементов

**Метод**:
- `ProcessBackground(IPublishedElement settings, Guid componentId, string prefix)`
- Возвращает: `BackgroundResult` с CSS классом

**Алгоритм**:
```
1. Извлечение настроек фона из IPublishedElement
2. Генерация уникального CSS класса на основе componentId
3. Создание CSS правил для фона
4. Кэширование CSS
5. Возврат BackgroundResult с классом
```

### 5.3 IStaticCssGeneratorService

**Назначение**: Генерация статических CSS файлов для фонов

**Метод**:
- `GenerateBackgroundCssFileAsync()`: Генерация CSS файла
- Возвращает: Путь к сгенерированному файлу

**Использование**: Для pre-generation CSS при деплое

### 5.4 ILoggingService

**Назначение**: Централизованное логирование

**Свойства**:
- `IsEnabled`: Флаг включения логирования

**Методы** (предполагаемые):
- `LogInformation(string message)`
- `LogWarning(string message)`
- `LogError<T>(string message, Exception ex)`

**Конфигурация** (Serilog):
- Console sink с цветами
- File sink с ротацией по дням (7 дней хранения)
- Event Log sink (опционально)
- Уровни логирования настраиваются в appsettings.json

## 6. Интеграции

### 6.1 Umbraco CMS

**Версия**: 17.1.0

**Ключевые компоненты**:

1. **Models Builder**:
   - Режим: SourceCodeAuto
   - Namespace: Umbraco.Cms.Web.Common.PublishedModels
   - Директория: ~/umbraco/models
   - Автоматическая генерация моделей при изменении Document Types

2. **Delivery API**:
   - Включен: true
   - Публичный доступ: true
   - Для headless сценариев

3. **ImageSharp**:
   - Кэш: ~/App_Data/TEMP/MediaCache
   - Максимальный возраст кэша: 365 дней
   - Максимальный возраст в браузере: 7 дней
   - Длина хэша: 12 символов

### 6.2 uSync

**Версия**: 17.0.2

**Назначение**: Синхронизация структуры контента через файлы

**Синхронизируемые элементы**:
- Document Types
- Data Types
- Templates
- Media Types
- Member Types
- Languages

**Преимущества**:
- Версионирование структуры в Git
- Синхронизация между окружениями
- Командная работа

### 6.3 Block Preview

**Версия**: 5.2.1

**Конфигурация**:
```json
{
  "BlockGrid": {
    "Enabled": true,
    "Stylesheets": [
      "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css",
      "/css/style.css",
      "/css/backgrounds.css",
      "/css/lazy-load.css",
      "/css/backoffice-preview.css"
    ],
    "Scripts": ["/js/lazy-load.js"]
  }
}
```

**Назначение**: Предпросмотр Block Grid и Block List в бэк-офисе

### 6.4 SEO Toolkit

**Версия**: 6.0.1

**Возможности**:
- Мета-поля для SEO
- Генерация sitemap.xml
- Аудит сайта
- Script manager
- Robots.txt управление

### 6.5 reCAPTCHA

**Версия**: 3.0.10

**Конфигурация**:
```json
{
  "RecaptchaSettings": {
    "SecretKey": "***",
    "SiteKey": "***"
  }
}
```

**Использование**:
- Атрибут `[ValidateRecaptcha(0.5)]` на методах контроллера
- Минимальный score: 0.5
- Защита форм от спама

## 7. Конфигурация

### 7.1 Структура конфигурации

**Файлы**:
- `appsettings.json`: Базовая конфигурация
- `appsettings.Development.json`: Переопределения для разработки
- Environment Variables: Переопределения для production

**Иерархия** (от низкого к высокому приоритету):
1. appsettings.json
2. appsettings.{Environment}.json
3. Environment Variables
4. Command Line Arguments

### 7.2 Ключевые секции

#### 7.2.1 Logging

```json
{
  "Logging": {
    "Enabled": true,
    "Level": "Information",
    "Console": { "Enabled": true, "UseColors": true },
    "File": {
      "Enabled": true,
      "Path": "Logs/log-.txt",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 7
    }
  }
}
```

#### 7.2.2 ImageOptimization

```json
{
  "ImageOptimization": {
    "DefaultWidths": [400, 600, 800, 1000, 1200, 1920],
    "WebpQuality": 80,
    "JpegQuality": 85,
    "EnableWebp": true,
    "EnableLazyLoading": true
  }
}
```

#### 7.2.3 Umbraco.CMS

Основные настройки:
- ModelsBuilder: SourceCodeAuto
- Runtime Mode: Development
- Delivery API: Enabled
- ImageSharp Cache: 365 дней

### 7.3 Чувствительные данные

**Хранение**:
- Development: appsettings.Development.json (не коммитится)
- Production: Environment Variables или Azure Key Vault

**Чувствительные параметры**:
- ConnectionStrings:umbracoDbDSN
- Email:EmailSenderPassword
- RecaptchaSettings:SecretKey

## 8. Развертывание

### 8.1 Docker

**Dockerfile** (multi-stage build):

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "TestUmbraco.dll"]
```

**docker-compose.yml**:

```yaml
version: '3.8'
services:
  web:
    build: .
    ports:
      - "801:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./TestUmbraco/wwwroot/media:/app/wwwroot/media
    depends_on:
      - db
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Str0ngP@ssw0rd!2024
    ports:
      - "1433:1433"
```

### 8.2 Требования к окружению

**Минимальные требования**:
- .NET 10.0 Runtime
- SQL Server 2019+
- 2 GB RAM
- 10 GB дисковое пространство

**Рекомендуемые требования**:
- .NET 10.0 Runtime
- SQL Server 2022
- 4 GB RAM
- 50 GB дисковое пространство (для медиа)

### 8.3 Процесс развертывания

1. **Подготовка**:
   - Установить .NET 10.0 SDK
   - Настроить SQL Server
   - Создать базу данных

2. **Конфигурация**:
   - Скопировать appsettings.json
   - Настроить ConnectionStrings
   - Настроить Email и reCAPTCHA

3. **Сборка**:
   ```bash
   dotnet restore
   dotnet build -c Release
   dotnet publish -c Release -o ./publish
   ```

4. **Миграции**:
   ```bash
   dotnet ef database update --startup-project TestUmbraco
   ```

5. **Запуск**:
   ```bash
   cd publish
   dotnet TestUmbraco.dll
   ```

6. **Проверка**:
   - Открыть http://localhost:5000
   - Проверить бэк-офис: http://localhost:5000/umbraco
   - Проверить логи в Logs/

## 9. Безопасность

### 9.1 Аутентификация

**Механизм**: Встроенная система Umbraco

**Особенности**:
- Cookie-based аутентификация для бэк-офиса
- JWT токены для API (опционально)
- Хэширование паролей (ASP.NET Core Identity)

### 9.2 Авторизация

**Политики**:
- `BackOffice`: Доступ к административным API
- Используется атрибут `[Authorize(Policy = "BackOffice")]`

### 9.3 Защита от атак

**SQL Injection**:
- Параметризованные запросы через EF Core
- Никаких сырых SQL строк с конкатенацией

**XSS**:
- Автоматическое экранирование в Razor
- Использование `IHtmlContent` для безопасного HTML

**CSRF**:
- Встроенная защита ASP.NET Core
- Anti-forgery токены в формах

**reCAPTCHA**:
- Защита форм от спама
- Минимальный score: 0.5

### 9.4 HTTPS

**Development**: HTTP (localhost)

**Production**: HTTPS обязательно
- Настройка через Kestrel или reverse proxy (nginx, IIS)
- HSTS заголовки
- Перенаправление HTTP → HTTPS

## 10. Производительность

### 10.1 Кэширование

**Уровни кэширования**:

1. **Browser Cache**:
   - Изображения: 7 дней
   - Статические файлы: настраивается

2. **Server Cache (ImageSharp)**:
   - Директория: ~/App_Data/TEMP/MediaCache
   - TTL: 365 дней
   - Максимум папок: 1000

3. **In-Memory Cache**:
   - HTML изображений
   - CSS фонов
   - Размер: 1024 MB
   - Compaction: 20%

4. **Response Caching**:
   - Максимальный размер тела: 64 KB
   - Общий лимит: 256 MB

### 10.2 Оптимизация изображений

**Стратегии**:
- Responsive images с srcset
- WebP с fallback
- Lazy loading
- Async decoding
- Кэширование сгенерированного HTML

**Размеры**:
- 6 размеров: 400, 600, 800, 1000, 1200, 1920
- Автоматический выбор оптимального размера браузером

### 10.3 Сжатие

**Response Compression**:
- Gzip compression
- Уровень: Optimal
- Включено для HTTPS

**Minification**:
- CSS и JavaScript минифицируются в production
- Cache buster: Timestamp

### 10.4 Асинхронность

**Принцип**: Все I/O операции асинхронные

**Примеры**:
- `await context.SaveChangesAsync()`
- `await File.ReadAllTextAsync()`
- `await httpClient.GetAsync()`
- `await _emailService.SendEmailRequestAsync()`

## 11. Мониторинг и логирование

### 11.1 Serilog

**Sinks**:
1. Console: Цветной вывод с timestamp
2. File: Ротация по дням, хранение 7 дней
3. Event Log: Только Warning и выше (опционально)

**Уровни логирования**:
- Debug: Детальная информация для отладки
- Information: Обычные операции
- Warning: Потенциальные проблемы
- Error: Ошибки, требующие внимания
- Critical: Критические ошибки

**Enrichers**:
- FromLogContext: Контекстная информация
- WithMachineName: Имя машины
- WithThreadId: ID потока

### 11.2 Логируемые события

**Контроллеры**:
- Входящие запросы
- Ошибки обработки
- Результаты операций

**Сервисы**:
- Операции с кэшем
- Отправка email
- Генерация CSS

**Umbraco**:
- Routing
- Document URL Service
- Packaging

### 11.3 Мониторинг производительности

**Application Insights** (опционально):
- InstrumentationKey в конфигурации
- Adaptive Sampling: отключен
- Performance Counters: отключены

**Метрики**:
- Время отклика API
- Использование памяти
- Количество запросов
- Ошибки и исключения

## 12. Тестирование

### 12.1 Стратегия тестирования

**Уровни**:
1. Unit Tests: Тестирование отдельных компонентов
2. Integration Tests: Тестирование взаимодействия компонентов
3. E2E Tests: Тестирование пользовательских сценариев

### 12.2 Тестируемые компоненты

**Сервисы**:
- EmailService: Отправка email
- MediaCacheService: Кэширование
- BackgroundService: Генерация CSS

**Helpers**:
- ImageHelper: Генерация responsive изображений
- Проверка корректности srcset и sizes

**Контроллеры**:
- API endpoints
- Обработка ошибок
- Валидация

### 12.3 Инструменты

**Фреймворки**:
- xUnit или NUnit
- Moq для mocking
- FluentAssertions для assertions

**Покрытие**:
- Цель: 80%+ для критичных компонентов
- Инструмент: Coverlet

## 13. Расширяемость

### 13.1 Добавление нового контроллера

```csharp
[Route("api/[controller]")]
[ApiController]
public class MyController : ControllerBase
{
    private readonly IMyService _myService;
    
    public MyController(IMyService myService)
    {
        _myService = myService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _myService.GetDataAsync();
        return Ok(result);
    }
}
```

### 13.2 Добавление нового сервиса

**1. Создать интерфейс**:
```csharp
// TestUmbraco.Application/Contracts/IMyService.cs
public interface IMyService
{
    Task<MyData> GetDataAsync();
}
```

**2. Создать реализацию**:
```csharp
// TestUmbraco.Application/Services/MyService.cs
public class MyService : IMyService
{
    public async Task<MyData> GetDataAsync()
    {
        // Реализация
    }
}
```

**3. Зарегистрировать в Composer**:
```csharp
public class MyComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<IMyService, MyService>();
    }
}
```

### 13.3 Добавление нового плагина

**1. Создать структуру**:
```
App_Plugins/MyPlugin/
├── my-plugin.js
├── my-plugin.css
└── umbraco-package.json
```

**2. Манифест** (umbraco-package.json):
```json
{
  "name": "My Plugin",
  "version": "1.0.0",
  "propertyEditors": [
    {
      "alias": "MyPlugin.Editor",
      "name": "My Editor",
      "editor": {
        "view": "~/App_Plugins/MyPlugin/my-plugin.html"
      }
    }
  ]
}
```

**3. Настроить копирование в .csproj**:
```xml
<ItemGroup>
  <Content Include="App_Plugins\MyPlugin\**" 
           CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## 14. Известные проблемы и ограничения

### 14.1 .NET 10.0

**Проблема**: Preview/RC версия может быть нестабильной

**Решение**: 
- Тщательное тестирование
- Готовность к откату на .NET 8.0
- Мониторинг release notes

### 14.2 Производительность при большом количестве изображений

**Проблема**: Генерация множества размеров может быть медленной

**Решение**:
- Кэширование сгенерированного HTML
- Асинхронная обработка
- CDN для статических файлов

### 14.3 Razor Compilation

**Особенность**: RazorCompileOnBuild включен для Models Builder

**Влияние**: Увеличивает время сборки

**Необходимость**: Требуется для SourceCodeAuto режима

## 15. Будущие улучшения

### 15.1 Краткосрочные (1-3 месяца)

- Добавление unit тестов для критичных компонентов
- Настройка CI/CD pipeline
- Оптимизация кэширования
- Добавление health checks

### 15.2 Среднесрочные (3-6 месяцев)

- Миграция на стабильную версию .NET
- Интеграция с CDN
- Добавление мониторинга производительности
- Расширение API

### 15.3 Долгосрочные (6-12 месяцев)

- Headless CMS режим
- Микросервисная архитектура
- Kubernetes deployment
- Расширенная аналитика

## 16. Заключение

TestUmbraco представляет собой современное веб-приложение на базе Umbraco CMS с акцентом на производительность, оптимизацию медиа-контента и расширяемость. Архитектура построена на принципах Clean Architecture с четким разделением ответственности между слоями.

**Ключевые преимущества**:
- Многослойная архитектура для масштабируемости
- Оптимизация изображений с WebP и responsive images
- Кэширование на всех уровнях
- Расширяемость через плагины и сервисы
- Современный стек технологий

**Технологическая база**:
- .NET 10.0 + ASP.NET Core
- Umbraco CMS 17.1.0
- Entity Framework Core 10.0.0
- TypeScript + Lit для плагинов
- Docker для контейнеризации

Система готова к production использованию и дальнейшему развитию.
