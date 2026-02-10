# Модульный обзор проекта TestUmbraco

## Введение

Этот документ представляет собой карту проекта TestUmbraco, организованную по логическим модулям. Каждый модуль объединяет связанные по функционалу компоненты и имеет четко определенную ответственность.

## Архитектурные слои

Проект построен на основе многослойной архитектуры:

```
┌─────────────────────────────────────────┐
│   Presentation Layer (TestUmbraco)      │
│   Controllers, Views, Helpers, Plugins  │
└─────────────────────────────────────────┘
              ↓ зависит от
┌─────────────────────────────────────────┐
│   Application Layer                     │
│   (TestUmbraco.Application)             │
│   Services, DTO, Business Logic         │
└─────────────────────────────────────────┘
              ↓ зависит от
┌─────────────────────────────────────────┐
│   Domain Layer (TestUmbraco.Domain)     │
│   Models, Repositories, DbContext       │
└─────────────────────────────────────────┘
```

---

## Модуль 1: Оптимизация и кэширование изображений

### Цель модуля
Обеспечение оптимизации, кэширования и адаптивного рендеринга изображений для повышения производительности сайта.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/Helpers/ImageHelper.cs` - Генерация responsive изображений
- `TestUmbraco/Services/IMediaCacheService.cs` - Интерфейс сервиса кэширования
- `TestUmbraco/Services/MediaCacheService.cs` - Реализация кэширования медиа
- `TestUmbraco/Models/ImageModel.cs` - Модель изображения
- `TestUmbraco/Models/Configuration/ImageOptimizationSettings.cs` - Настройки оптимизации
- `TestUmbraco/Controllers/MediaCacheController.cs` - API управления кэшем
- `TestUmbraco/Composers/MediaCacheComposer.cs` - Регистрация сервисов
- `TestUmbraco/Views/Shared/_Image.cshtml` - Partial view для изображений
- `TestUmbraco/wwwroot/css/images.css` - Стили для изображений
- `TestUmbraco/wwwroot/css/lazy-load.css` - Стили для lazy loading
- `TestUmbraco/wwwroot/js/lazy-load.js` - JavaScript для lazy loading

### Основные классы и их роль

**ImageHelper**
- `RenderResponsiveMediaAsync()` - Генерация адаптивных изображений с srcset
- `GenerateImageUrl()` - Создание URL с параметрами оптимизации
- `GenerateSrcSet()` - Генерация srcset для разных размеров
- `GenerateSizesFromGrid()` - Расчет sizes на основе Bootstrap grid
- `RenderPlaceholderAsync()` - Placeholder при отсутствии изображения

**MediaCacheService**
- `GetCachedMediaUrlAsync()` - Получение URL с кэшированием
- `GetCachedImageHtmlAsync()` - Генерация HTML с кэшированием
- `GeneratePictureElement()` - Создание элемента `<picture>`
- `ConvertToWebP()` - Конвертация в WebP формат
- `ClearCacheForMedia()` - Очистка кэша для конкретного медиа

**MediaCacheController**
- `GetCacheInfo()` - Информация о состоянии кэша
- `ClearCacheForMedia()` - Очистка кэша по GUID
- `ClearAllMediaCache()` - Полная очистка кэша

**ImageOptimizationSettings**
- Конфигурация размеров, качества WebP/JPEG, lazy loading

### Зависимости от других модулей
- **Umbraco CMS Core**: Использует `IPublishedContent`, `IMediaService`
- **Модуль логирования**: Для записи ошибок
- **Модуль конфигурации**: Для настроек оптимизации

---

## Модуль 2: Управление фоновыми изображениями

### Цель модуля
Динамическая генерация CSS для фоновых изображений, цветов, градиентов и видео с поддержкой оверлеев и lazy loading.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/Services/IUmbracoBackgroundService.cs` - Интерфейс сервиса фонов
- `TestUmbraco/Services/UmbracoBackgroundService.cs` - Обработка фонов
- `TestUmbraco/Services/IStaticCssGeneratorService.cs` - Интерфейс генератора CSS
- `TestUmbraco/Services/StaticCssGeneratorService.cs` - Генерация статических CSS
- `TestUmbraco/Services/BackgroundClassesService.cs` - Вспомогательный сервис
- `TestUmbraco/Services/BackgroundResult.cs` - Результат обработки фона
- `TestUmbraco/Services/BackgroundInfo.cs` - Информация о фоне
- `TestUmbraco/Models/BackgroundClassesModel.cs` - Модель CSS классов
- `TestUmbraco/Controllers/UniversalBackgroundController.cs` - API для CSS фонов
- `TestUmbraco/Helpers/HtmlHelper.cs` - Extension методы для фонов
- `TestUmbraco/Views/Shared/_BackgroundClasses.cshtml` - Partial view
- `TestUmbraco/Views/Partials/_BackgroundConfig.cshtml` - Конфигурация фонов
- `TestUmbraco/wwwroot/css/backgrounds.css` - Сгенерированные стили
- `TestUmbraco/wwwroot/js/lazy-backgrounds.js` - Lazy loading фонов
- `TestUmbraco/wwwroot/js/video-placeholders.js` - Placeholders для видео
- `TestUmbraco/App_Plugins/UniversalBackground/` - Плагин для бэк-офиса

### Основные классы и их роль

**UmbracoBackgroundService**
- `ProcessBackground()` - Главный метод обработки фонов
- `ProcessImageBackground()` - Обработка фоновых изображений
- `ProcessColorBackground()` - Обработка цветовых фонов
- `ProcessGradientBackground()` - Обработка градиентов
- `ProcessVideoBackground()` - Обработка видео фонов (Vimeo)
- `AddOverlayStyles()` - Добавление стилей оверлея
- `RegisterBackgroundInfo()` - Регистрация данных для JavaScript

**StaticCssGeneratorService**
- `GenerateBackgroundCssFileAsync()` - Генерация базового CSS файла
- `GetOrAddMediaClassAsync()` - Добавление класса для медиа
- `GetOrAddColorClassAsync()` - Добавление класса для цвета
- `GetOrAddGradientClassAsync()` - Добавление класса для градиента
- `AddInlineStyleAsync()` - Добавление inline стилей
- `AddOverlayStyleAsync()` - Добавление стилей оверлея
- `UpdateCssForMediaAsync()` - Обновление CSS при изменении медиа

**UniversalBackgroundController**
- `GetMultipleCss()` - API endpoint для получения CSS нескольких фонов

**BackgroundHtmlExtensions**
- `GetBackgroundClasses()` - Extension метод для получения CSS классов
- `AddBackgroundClasses()` - Объединение базовых и фоновых классов

### Зависимости от других модулей
- **Модуль оптимизации изображений**: Использует `IMediaCacheService`
- **Модуль логирования**: Для отладки и ошибок
- **Umbraco CMS Core**: Работа с `IPublishedElement`, `IMediaService`

---

## Модуль 3: Отправка Email

### Цель модуля
Обработка форм обратной связи и отправка email уведомлений с защитой от спама через reCAPTCHA.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/Controllers/EmailController.cs` - Контроллер форм

**Application Layer:**
- `TestUmbraco.Application/Contracts/IEmailService.cs` - Интерфейс сервиса
- `TestUmbraco.Application/Services/EmailService.cs` - Реализация отправки
- `TestUmbraco.Application/DTO/EmailRequestDto.cs` - DTO email-заявки
- `TestUmbraco.Application/DTO/CallRequestDto.cs` - DTO запроса звонка

**Domain Layer:**
- `TestUmbraco.Domain/Models/EmailRequestItem.cs` - Entity email-заявки
- `TestUmbraco.Domain/Models/CallRequestItem.cs` - Entity запроса звонка

### Основные классы и их роль

**EmailController** (SurfaceController)
- `SendEmailRequest()` - Обработка email-заявки
- `SendCallRequest()` - Обработка запроса звонка
- Использует атрибут `[ValidateRecaptcha(0.5)]` для защиты

**EmailService**
- `SendEmailRequestAsync()` - Отправка email-заявки
- `SendCallRequestAsync()` - Отправка запроса звонка
- `SendEmailAsync()` - Базовый метод отправки через SMTP

**EmailRequestDto / CallRequestDto**
- DTO для передачи данных между слоями

**EmailRequestItem / CallRequestItem**
- Entity модели для сохранения в БД
- Наследуются от `EntityBase`

### Зависимости от других модулей
- **Модуль данных**: Использует `IRepository<T>` для сохранения
- **Модуль конфигурации**: SMTP настройки из appsettings.json
- **Внешние библиотеки**: MailKit, MimeKit, reCAPTCHA.AspNetCore

---

## Модуль 4: Работа с данными (Data Access)

### Цель модуля
Абстракция доступа к данным через Repository Pattern и управление контекстом Entity Framework.

### Ключевые файлы и папки

**Domain Layer:**
- `TestUmbraco.Domain/AppDbContext.cs` - DbContext Entity Framework
- `TestUmbraco.Domain/Contracts/IRepository.cs` - Интерфейс репозитория
- `TestUmbraco.Domain/Repositories/Repository.cs` - Реализация репозитория
- `TestUmbraco.Domain/Models/EntityBase.cs` - Базовая entity
- `TestUmbraco.Domain/Models/EmailRequestItem.cs` - Entity заявки
- `TestUmbraco.Domain/Models/CallRequestItem.cs` - Entity звонка
- `TestUmbraco.Domain/Migrations/` - EF Core миграции

### Основные классы и их роль

**AppDbContext**
- DbContext для работы с SQL Server
- Управление DbSet для всех entity
- Конфигурация подключения к БД

**IRepository<T>**
- Интерфейс generic репозитория
- `AddAsync()` - Добавление entity

**Repository<T>**
- Реализация generic репозитория
- Работа с DbContext
- Сохранение изменений

**EntityBase**
- Базовый класс для всех entity
- Свойства: Id, CreatedAt, UpdatedAt, IsActive
- Обеспечивает единообразие моделей

### Зависимости от других модулей
- **Entity Framework Core**: ORM для работы с БД
- **SQL Server**: База данных

---

## Модуль 5: Логирование

### Цель модуля
Централизованное структурированное логирование с поддержкой цветного вывода в консоль и записи в файлы.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/Services/ILoggingService.cs` - Интерфейс сервиса
- `TestUmbraco/Services/LoggingService.cs` - Реализация логирования
- `TestUmbraco/Logs/` - Директория с логами

### Основные классы и их роль

**ILoggingService**
- `IsEnabled` - Флаг включения логирования
- Методы для разных уровней логирования

**LoggingService**
- `LogInformation()` / `LogInformation<T>()` - Информационные сообщения
- `LogError()` / `LogError<T>()` - Ошибки
- `LogWarning()` / `LogWarning<T>()` - Предупреждения
- `WriteColored()` - Цветной вывод в консоль

### Зависимости от других модулей
- **Serilog**: Структурированное логирование
- **Модуль конфигурации**: Настройки логирования

---

## Модуль 6: Кастомные плагины бэк-офиса

### Цель модуля
Расширение функционала бэк-офиса Umbraco через кастомные property editors.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/App_Plugins/MenuIdPicker/` - Плагин выбора ID меню
  - `menu-id-picker.js` - JavaScript компонент
  - `menu-id-picker.css` - Стили
  - `umbraco-package.json` - Манифест
- `TestUmbraco/App_Plugins/Vokseverk.ColorSelector/` - Плагин выбора цвета
  - `color-selector.js` - JavaScript компонент
  - `umbraco-package.json` - Манифест
- `TestUmbraco/App_Plugins/UniversalBackground/` - Плагин управления фонами
  - Компилируется из TypeScript
  - Использует Lit Web Components

**Source Files:**
- `plugins-src/UniversalBackground/` - Исходники TypeScript

### Основные компоненты и их роль

**MenuIdPicker**
- Property editor для выбора ID секции меню
- Интеграция с Umbraco бэк-офисом

**ColorSelector**
- Property editor для выбора цвета
- Цветовая палитра

**UniversalBackground**
- Property editor для управления фоновыми изображениями
- Настройка параметров фона (size, position, overlay)
- TypeScript + Lit компоненты

### Зависимости от других модулей
- **Umbraco Backoffice**: @umbraco-cms/backoffice
- **Lit**: Для Web Components
- **TypeScript**: Для типобезопасности

---

## Модуль 7: Umbraco Content Models

### Цель модуля
Автогенерированные модели контента для типобезопасной работы с Document Types.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/umbraco/models/*.generated.cs` - Сгенерированные модели
  - `Page.generated.cs` - Модель страницы
  - `BlockGrid.generated.cs` - Модель Block Grid
  - `GridSection.generated.cs` - Модель секции
  - `GridColumn.generated.cs` - Модель колонки
  - `Hero.generated.cs`, `Card.generated.cs`, и т.д. - Компоненты

### Основные модели и их роль

**Page**
- Основная модель страницы сайта
- Свойства: Title, Content, SEO метаданные

**BlockGrid / GridSection / GridColumn**
- Модели для Grid редактора
- Структура layout страницы

**Компоненты** (Hero, Card, Img, Video, Quote, и т.д.)
- Модели для Block Grid компонентов
- Переиспользуемые блоки контента

### Зависимости от других модулей
- **Umbraco Models Builder**: Автогенерация моделей
- **uSync**: Синхронизация Document Types

---

## Модуль 8: Views и Razor Templates

### Цель модуля
Рендеринг HTML контента с использованием Razor шаблонов и partial views.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/Views/` - Основные views
  - `Layout.cshtml` - Главный layout
  - `BlockGrid.cshtml` - View для Block Grid
  - `_ViewImports.cshtml` - Импорты и using
- `TestUmbraco/Views/Partials/` - Partial views
  - `blockgrid/Components/` - Компоненты Block Grid
  - `blocklist/Components/` - Компоненты Block List
  - `blockpreview/` - Предпросмотр в бэк-офисе
  - `layout/` - Элементы layout (меню, favicon, и т.д.)
- `TestUmbraco/Views/Shared/` - Общие partial views
  - `_BackgroundClasses.cshtml` - Рендеринг фонов
  - `_Image.cshtml` - Рендеринг изображений

### Основные views и их роль

**Layout.cshtml**
- Главный layout страницы
- Подключение CSS, JavaScript
- Меню, footer, метатеги

**BlockGrid.cshtml**
- Рендеринг Block Grid структуры
- Итерация по секциям и колонкам

**Компоненты** (gridSection, gridColumn, img, video, и т.д.)
- Рендеринг отдельных блоков
- Использование helpers для изображений и фонов

**Block Preview**
- Предпросмотр компонентов в бэк-офисе
- Подключение стилей Bootstrap и кастомных CSS

### Зависимости от других модулей
- **Модуль оптимизации изображений**: ImageHelper
- **Модуль фонов**: BackgroundHtmlExtensions
- **Umbraco Content Models**: Типизированные модели

---

## Модуль 9: Статические ресурсы (Frontend Assets)

### Цель модуля
CSS стили и JavaScript для фронтенда сайта.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/wwwroot/css/` - Стили
  - `style.css` - Основные стили сайта
  - `backgrounds.css` - Сгенерированные стили фонов
  - `images.css` - Стили для изображений
  - `lazy-load.css` - Стили для lazy loading
  - `backoffice-preview.css` - Стили для предпросмотра
- `TestUmbraco/wwwroot/js/` - JavaScript
  - `lazy-load.js` - Lazy loading изображений
  - `lazy-backgrounds.js` - Lazy loading фонов
  - `video-placeholders.js` - Placeholders для видео
  - `site-menu.js` - Логика меню
  - `button-up.js` - Кнопка "Вверх"

### Основные скрипты и их роль

**lazy-load.js**
- Intersection Observer для изображений
- Загрузка изображений при появлении в viewport

**lazy-backgrounds.js**
- Lazy loading фоновых изображений
- Загрузка видео фонов (Vimeo)
- Обработка placeholders

**video-placeholders.js**
- Управление video placeholders
- Переключение между видео и изображением на мобильных

**site-menu.js**
- Логика навигационного меню
- Мобильное меню

### Зависимости от других модулей
- **Bootstrap 5.3.0**: CSS фреймворк (CDN)
- **Модуль фонов**: Использует данные из BackgroundInfo

---

## Модуль 10: Конфигурация и настройки

### Цель модуля
Централизованное управление конфигурацией приложения.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/appsettings.json` - Основная конфигурация
- `TestUmbraco/appsettings.Development.json` - Конфигурация для разработки
- `TestUmbraco/Program.cs` - Точка входа приложения

### Основные секции конфигурации

**Logging**
- Настройки Serilog
- Уровни логирования
- Sinks (Console, File, EventLog)

**ImageOptimization**
- DefaultWidths, WebpQuality, JpegQuality
- EnableWebp, EnableLazyLoading

**Email**
- SMTP настройки
- Отправитель и получатель

**RecaptchaSettings**
- SecretKey, SiteKey

**Umbraco.CMS**
- ModelsBuilder, Imaging, DeliveryApi
- Runtime, Security, WebRouting

**ConnectionStrings**
- umbracoDbDSN - подключение к SQL Server

**BlockPreview**
- Настройки предпросмотра
- Подключаемые стили и скрипты

### Зависимости от других модулей
- Используется всеми модулями для получения настроек

---

## Модуль 11: Dependency Injection и Composers

### Цель модуля
Регистрация сервисов в DI контейнере через Umbraco Composers.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/Composers/MediaCacheComposer.cs` - Регистрация сервисов кэширования

### Основные классы и их роль

**MediaCacheComposer**
- Реализует `IComposer`
- Метод `Compose(IUmbracoBuilder builder)`
- Регистрирует:
  - `IMediaCacheService` → `MediaCacheService` (Singleton)
  - `IUmbracoBackgroundService` → `UmbracoBackgroundService`
  - `IStaticCssGeneratorService` → `StaticCssGeneratorService`
  - `ILoggingService` → `LoggingService`
  - `IEmailService` → `EmailService`
  - `IRepository<T>` → `Repository<T>`

### Зависимости от других модулей
- Регистрирует сервисы из всех модулей

---

## Модуль 12: uSync - Синхронизация контента

### Цель модуля
Синхронизация структуры контента Umbraco через файловую систему для версионирования и командной работы.

### Ключевые файлы и папки

**Presentation Layer:**
- `TestUmbraco/uSync/v17/` - Директория синхронизации
  - `Content/` - Контент страниц
  - `ContentTypes/` - Document Types
  - `DataTypes/` - Data Types
  - `Templates/` - Razor шаблоны
  - `MediaTypes/` - Media Types
  - `Languages/` - Языки
  - `Media/` - Медиа файлы
  - `MemberTypes/` - Member Types
  - `RelationTypes/` - Типы связей
  - `usync.config` - Конфигурация uSync

### Основные компоненты и их роль

**ContentTypes**
- Определения Document Types
- Структура полей и свойств

**DataTypes**
- Настройки property editors
- Конфигурация типов данных

**Templates**
- Экспорт Razor шаблонов
- Связь с Document Types

**Content**
- Экспорт контента страниц
- Для переноса между окружениями

### Зависимости от других модулей
- **Umbraco CMS**: Интеграция с ядром
- **Git**: Версионирование файлов

---

## Диаграмма зависимостей модулей

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Оптимизация │  │    Фоны      │  │   Плагины    │      │
│  │  изображений │◄─┤  изображений │  │  бэк-офиса   │      │
│  └──────┬───────┘  └──────┬───────┘  └──────────────┘      │
│         │                 │                                  │
│         │                 │                                  │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌──────────────┐      │
│  │ Логирование  │  │ Конфигурация │  │    Views     │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Email Forms  │  │   uSync      │  │   Frontend   │      │
│  └──────┬───────┘  └──────────────┘  └──────────────┘      │
└─────────┼──────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│                                                              │
│  ┌──────────────┐                                           │
│  │ Email Service│                                           │
│  └──────┬───────┘                                           │
└─────────┼──────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Repositories │  │   Entities   │  │  DbContext   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────┐
│                      SQL Server Database                     │
└─────────────────────────────────────────────────────────────┘
```

---

## Межмодульные зависимости

### Модуль оптимизации изображений
- **Использует**: Логирование, Конфигурация, Umbraco Core
- **Используется**: Модуль фонов, Views

### Модуль фонов
- **Использует**: Оптимизация изображений, Логирование, Конфигурация
- **Используется**: Views, Frontend Assets

### Модуль Email
- **Использует**: Модуль данных, Конфигурация, Логирование
- **Используется**: Email Forms Controller

### Модуль данных
- **Использует**: Entity Framework Core, SQL Server
- **Используется**: Модуль Email

### Модуль логирования
- **Использует**: Serilog, Конфигурация
- **Используется**: Все модули

### Модуль конфигурации
- **Используется**: Все модули

---

## Рекомендации для новых разработчиков

### Начало работы

1. **Изучите архитектуру**: Понимание слоев (Presentation → Application → Domain) критично
2. **Начните с модуля данных**: Понимание Entity и Repository Pattern
3. **Изучите модуль конфигурации**: Все настройки в appsettings.json
4. **Ознакомьтесь с Umbraco**: Базовые концепции CMS

### Добавление нового функционала

1. **Определите модуль**: К какому модулю относится функционал?
2. **Создайте интерфейс**: В соответствующем слое (Contracts/)
3. **Реализуйте сервис**: В Services/
4. **Зарегистрируйте в DI**: Через Composer
5. **Создайте контроллер**: Если нужен API endpoint
6. **Добавьте view**: Если нужен UI

### Отладка

1. **Проверьте логи**: `TestUmbraco/Logs/log-YYYYMMDD.txt`
2. **Используйте LoggingService**: Для отладочных сообщений
3. **Проверьте кэш**: Очистите через MediaCacheController API
4. **Проверьте CSS**: `wwwroot/css/backgrounds.css` для фонов

### Тестирование

1. **Unit тесты**: Для сервисов и helpers
2. **Integration тесты**: Для контроллеров и репозиториев
3. **E2E тесты**: Для пользовательских сценариев

---

## Заключение

Проект TestUmbraco организован в 12 логических модулей, каждый из которых имеет четко определенную ответственность. Модульная структура обеспечивает:

- **Разделение ответственности**: Каждый модуль решает свою задачу
- **Переиспользование кода**: Сервисы используются через интерфейсы
- **Тестируемость**: Модули можно тестировать независимо
- **Масштабируемость**: Легко добавлять новые модули
- **Поддерживаемость**: Понятная структура для новых разработчиков

Следуя этой карте, новый разработчик может быстро ориентироваться в проекте и понимать, где находится нужный функционал.
