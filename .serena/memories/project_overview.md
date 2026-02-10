# Обзор проекта TestUmbraco

## Назначение проекта
TestUmbraco - это веб-приложение на базе CMS Umbraco версии 17.1.0 с кастомными расширениями для управления фоновыми изображениями Grid-редактора и оптимизацией изображений. Проект включает в себя:
- Систему управления контентом на базе Umbraco CMS
- Кастомные плагины для бэк-офиса (MenuIdPicker, ColorSelector, UniversalBackground)
- Систему оптимизации и кэширования изображений
- Email-сервис с поддержкой reCAPTCHA
- Интеграцию с SEO Toolkit

## Технологический стек

### Backend
- **Framework**: ASP.NET Core на .NET 10.0
- **CMS**: Umbraco CMS 17.1.0
- **База данных**: Microsoft SQL Server (через Entity Framework Core 10.0.0)
- **ORM**: Entity Framework Core 10.0.0
- **Email**: MailKit 4.7.1, MimeKit 4.7.1
- **Безопасность**: reCAPTCHA.AspNetCore 3.0.10, JWT tokens
- **Дополнительные пакеты**:
  - uSync 17.0.2 (синхронизация контента)
  - SeoToolkit.Umbraco 6.0.1
  - Umbraco.Community.BlockPreview 5.2.1
  - AspNetCoreHero.ToastNotification 1.1.0

### Frontend
- **TypeScript**: 5.9.3
- **Lit**: 3.3.2 (для веб-компонентов)
- **Umbraco Backoffice**: @umbraco-cms/backoffice 16.5.0
- **UI Components**: @umbraco-ui/uui 1.16.0
- **CSS Framework**: Bootstrap 5.3.0

### Инфраструктура
- **Контейнеризация**: Docker (Dockerfile + docker-compose.yml)
- **Runtime**: .NET 8.0 SDK для сборки, ASP.NET Core 8.0 для runtime
- **Порт**: 801 (маппинг на 80 внутри контейнера)

## Архитектура решения

Проект использует многослойную архитектуру с разделением на три основных проекта:

### 1. TestUmbraco (Web/Presentation Layer)
Основной веб-проект с Umbraco CMS:
- **Controllers**: API-контроллеры (EmailController, MediaCacheController, UniversalBackgroundController)
- **Helpers**: Вспомогательные классы (HtmlHelper, ImageHelper)
- **Models**: Модели данных и конфигурации
- **Composers**: Композеры для регистрации сервисов
- **App_Plugins**: Кастомные плагины для бэк-офиса Umbraco
- **Views**: Razor-шаблоны
- **wwwroot**: Статические файлы (CSS, JS, изображения)

### 2. TestUmbraco.Application (Application/Service Layer)
Слой бизнес-логики:
- **Services**: Сервисы приложения
- **Contracts**: Интерфейсы сервисов
- **DTO**: Data Transfer Objects

### 3. TestUmbraco.Domain (Domain/Data Layer)
Слой данных и доменной логики:
- **Models**: Доменные модели
- **Repositories**: Репозитории для работы с данными
- **Contracts**: Интерфейсы репозиториев
- **Migrations**: Миграции базы данных
- **AppDbContext.cs**: Контекст Entity Framework

## Ключевые особенности

### Оптимизация изображений
- Автоматическая генерация responsive изображений с srcset
- Поддержка WebP формата
- Настраиваемые размеры и качество
- Lazy loading
- Кэширование через MediaCacheService

### Кастомные плагины Umbraco
- **MenuIdPicker**: Выбор ID меню
- **ColorSelector**: Выбор цвета (Vokseverk)
- **UniversalBackground**: Управление фоновыми изображениями

### Конфигурация
- Централизованная конфигурация в appsettings.json
- Настройки оптимизации изображений
- Email-конфигурация
- Настройки Umbraco CMS
- Логирование через Serilog

## База данных
- **Сервер**: SQL Server (localhost:1433)
- **База данных**: TestUmbraco
- **Провайдер**: Microsoft.Data.SqlClient
- **Миграции**: Entity Framework Core Migrations
