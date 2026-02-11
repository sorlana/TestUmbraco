# Спецификация статических ресурсов

## Правило: Всегда только 3 CSS файла

### 1. `backgrounds.css`
- **Назначение**: Динамически генерируемые стили для фонов
- **Генерация**: Автоматически через `StaticCssGeneratorService.cs`
- **Содержит**:
  - Базовые стили для `.lazy-bg`, `.lazy-video`
  - Динамические классы для медиа-фонов (`.bg-media-*`)
  - Динамические классы для цветов (`.bg-color-*`)
  - Динамические классы для градиентов (`.bg-gradient-*`)
  - Динамические стили для оверлеев (`.overlay-*`)
  - Динамические стили для видео (`.bg-video-*`)
- **НЕ РЕДАКТИРОВАТЬ ВРУЧНУЮ**: Файл перезаписывается автоматически

### 2. `backoffice-preview.css`
- **Назначение**: Переопределение стилей для Block Preview в Umbraco backoffice
- **Содержит**:
  - Переопределения z-index для правильного отображения слоев
  - Специфичные стили для `.umb-block-grid__layout-item` и `.umb-block-list__item`
  - Скрытие видео и показ placeholder в backoffice
- **Редактируется вручную** при необходимости изменить поведение в backoffice

### 3. `style.css`
- **Назначение**: Все статические стили сайта
- **Содержит**:
  - Базовые стили (typography, layout, scrollbar)
  - Стили изображений (`.image-fallback`, `.image-error`)
  - Стили видимости секций (`.section-hidden`)
  - Стили lazy loading (`.lazy-loading`, `.lazy-loaded`, `.lazy-error`)
  - Стили для фоновых изображений с data-bg
  - Анимации (shimmer)
  - Адаптивные стили
  - Все остальные компоненты сайта
- **Редактируется вручную** для добавления новых стилей

## Правило: Всегда только 1 JS файл

### `site.js`
- **Назначение**: Все JavaScript функциональности сайта
- **Содержит** (в порядке выполнения):
  1. **Button Up** - кнопка "Наверх"
  2. **Lazy Load** - универсальная ленивая загрузка изображений
  3. **Video Placeholders** - установка фоновых изображений для video placeholder
  4. **Site Menu** - меню и навигация (мобильное меню, smooth scroll, управление секциями)
  5. **Lazy Backgrounds** - фоновые изображения и видео (зависит от `window.lazyBackgroundsConfig`)
- **Структура**: Каждый модуль обернут в IIFE для изоляции
- **Зависимости**: 
  - jQuery (опционально, для анимаций)
  - `window.lazyBackgroundsConfig` (генерируется в `_BackgroundConfig.cshtml`)
- **Порядок загрузки**: ВАЖНО! Сначала `_BackgroundConfig`, потом `site.js`

## Удаленные файлы

### CSS (объединены в style.css):
- ~~`images.css`~~ → перенесено в `style.css`
- ~~`lazy-load.css`~~ → перенесено в `style.css`

### JS (объединены в site.js):
- ~~`button-up.js`~~ → перенесено в `site.js`
- ~~`lazy-load.js`~~ → перенесено в `site.js`
- ~~`video-placeholders.js`~~ → перенесено в `site.js`
- ~~`site-menu.js`~~ → перенесено в `site.js`
- ~~`lazy-backgrounds.js`~~ → перенесено в `site.js`

## Порядок загрузки в Layout.cshtml

```html
<head>
    <!-- 1. backgrounds.css - динамические фоны -->
    <link rel="stylesheet" href="~/css/backgrounds.css" asp-append-version="true" />
    
    <!-- 2. style.css - все статические стили -->
    <link rel="stylesheet" href="~/css/style.css" media="screen"/>
    
    <!-- Внешние ресурсы (шрифты, иконки) -->
    <link href="https://fonts.googleapis.com/..." rel="stylesheet">
</head>

<body>
    <!-- Контент -->
    
    <!-- ВАЖНО: Сначала конфиг, потом скрипт -->
    <partial name="_BackgroundConfig" />
    <script src="~/js/site.js" asp-append-version="true"></script>
</body>
```

## Правила добавления новых стилей

1. **Динамические стили** (зависят от контента Umbraco):
   - Добавлять через `StaticCssGeneratorService.cs`
   - Будут автоматически в `backgrounds.css`

2. **Статические стили** (не зависят от контента):
   - Добавлять в `style.css`
   - Группировать по функциональности с комментариями

3. **Стили для backoffice**:
   - Добавлять в `backoffice-preview.css`
   - Использовать селекторы `.umb-block-grid__layout-item` или `.umb-block-list__item`

## Правила добавления новых скриптов

1. **Новая функциональность**:
   - Добавлять в `site.js` как новый модуль
   - Оборачивать в IIFE: `(function() { 'use strict'; ... })();`
   - Добавлять комментарий с описанием модуля

2. **Зависимости**:
   - Если зависит от конфига: проверять `window.lazyBackgroundsConfig`
   - Если зависит от DOM: использовать `DOMContentLoaded` или проверку `document.readyState`

3. **Порядок модулей в site.js**:
   - Независимые модули (Button Up, Lazy Load) - в начале
   - Зависимые модули (Lazy Backgrounds) - в конце

## Производительность

### CSS
- **backgrounds.css**: Кешируется браузером, обновляется при изменении контента
- **style.css**: Кешируется браузером, редко меняется
- **backoffice-preview.css**: Загружается только в backoffice

### JS
- **site.js**: Один файл = один HTTP запрос
- Все модули инициализируются асинхронно
- Используется `passive: true` для scroll/resize listeners
- IntersectionObserver для lazy loading

## Отладка

### Проверка загрузки CSS:
```javascript
// В консоли браузера
document.styleSheets
```

### Проверка загрузки JS модулей:
```javascript
// В консоли браузера
window.lazyLoad // Lazy Load модуль
window.lazyBackgroundManager // Lazy Backgrounds модуль
```

### Debug режим:
- Откройте DevTools (F12)
- Вкладка Network → фильтр CSS/JS
- Проверьте что загружаются только 3 CSS и 1 JS файл

## Миграция существующего кода

При добавлении нового функционала:

1. **НЕ создавайте новые CSS файлы** → добавляйте в `style.css`
2. **НЕ создавайте новые JS файлы** → добавляйте модуль в `site.js`
3. **НЕ используйте inline стили** → используйте классы из CSS
4. **НЕ дублируйте код** → переиспользуйте существующие модули

## Исключения

Единственные допустимые дополнительные файлы:
- Внешние библиотеки (jQuery, если нужно)
- Внешние шрифты (Google Fonts)
- Внешние иконки (Material Icons)

Все остальное должно быть в трех CSS файлах и одном JS файле.
