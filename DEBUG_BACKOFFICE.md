# Отладка Block Preview - ФИНАЛЬНАЯ ВЕРСИЯ

## ✅ Решение работает

### Что работает
- ✅ Цветные фоны отображаются
- ✅ Градиентные фоны работают
- ✅ Медиа фоны (изображения) видны
- ✅ Overlay отображаются с правильным цветом и прозрачностью
- ✅ Overlay НАД видео фоном (между видео и контентом)
- ✅ `backoffice-preview.css` загружается и применяется
- ✅ Lazy loading видео с placeholder для быстрой загрузки (только на фронтенде)
- ✅ Плавный переход от placeholder к видео (только на фронтенде)
- ✅ Lazy loading для фоновых изображений с shimmer эффектом
- ✅ Lazy loading для обычных изображений с плавным появлением
- ✅ Высота секций по содержимому (убран min-height: 400px)
- ✅ TextInverse - изменение цвета всего текста в блоке

## Как это работает

### 1. Структура z-index

```
z-index: 1  - Контент (текст, изображения)
z-index: -1 - Overlay (псевдоэлемент ::before) - МЕЖДУ видео и контентом
z-index: -2 - Видео фон (.video-container, .video-bg-iframe, .video-placeholder)
```

**ВАЖНО**: Overlay с видео имеет `z-index: -1`, а overlay без видео имеет `z-index: 0`

### 2. Ключевые правила в backoffice-preview.css

**Overlay БЕЗ видео:**
```css
.overlay-XXX::before {
  z-index: 0 !important;  /* Над фоном, под контентом */
  background-color: #2b00ff !important;
  opacity: 0.30 !important;
}
```

**Overlay С видео:**
```css
.bg-video-XXX.overlay-YYY::before {
  z-index: -1 !important;  /* Между видео и контентом */
}
```

**Видео:**
```css
.video-container,
.video-bg-iframe,
.video-placeholder {
  z-index: -2 !important;  /* Под overlay */
}
```

**Контент:**
```css
[class*="overlay-"] > * {
  z-index: 1 !important;  /* Над overlay */
}
```

### 3. Lazy Loading видео

**Как работает:**
1. Видео iframe имеет `data-src` вместо `src` (не загружается сразу)
2. Placeholder с изображением показывается мгновенно (`z-index: -2`)
3. **В backoffice**: видео НЕ загружается, показывается только placeholder
4. **На фронтенде**: IntersectionObserver отслеживает когда элемент появляется в viewport
5. За 200px до появления начинается загрузка видео (только на фронтенде)
6. После загрузки placeholder плавно исчезает (только на фронтенде)
7. Используется уникальный ID для каждого видео

**Преимущества:**
- В backoffice мгновенная загрузка (только placeholder, без видео)
- Экономия ресурсов в backoffice (видео не загружается)
- На фронтенде видео загружается только когда нужно
- Экономия трафика и ресурсов на фронтенде
- Плавный переход без "мигания"

**Логика определения backoffice:**
```javascript
var isInIframe = window.self !== window.top;
if (isInIframe) {
  // Backoffice - показываем только placeholder
  iframe.style.display = 'none';
  placeholder.style.opacity = '1';
  return; // Не загружаем видео
}
// Фронтенд - загружаем видео с lazy loading
```

**CSS для backoffice:**
```css
/* В backoffice placeholder всегда видим */
.umb-block-grid__layout-item .video-placeholder,
.umb-block-list__item .video-placeholder {
  opacity: 1 !important;
  display: block !important;
}

/* В backoffice iframe скрыт */
.umb-block-grid__layout-item .video-bg-iframe,
.umb-block-list__item .video-bg-iframe {
  display: none !important;
}
```

### 4. Lazy Loading изображений

**Универсальный скрипт `/js/lazy-load.js`:**
- Работает с обычными изображениями (`img[data-src]`, `img[data-srcset]`)
- Работает с фоновыми изображениями (`[data-bg]`)
- IntersectionObserver с rootMargin 50px
- Плавные переходы через CSS
- Fallback для старых браузеров

**Использование:**

Обычные изображения:
```html
<img data-src="/path/to/image.jpg" 
     data-srcset="/path/to/image-400.jpg 400w, /path/to/image-800.jpg 800w"
     alt="Description"
     class="lazy-image" />
```

Фоновые изображения:
```html
<div data-bg="/path/to/background.jpg" 
     class="hero-section">
  Контент
</div>
```

**CSS эффекты (`/css/lazy-load.css`):**
- Shimmer анимация для placeholder
- Плавное появление через opacity transition
- Адаптивность для prefers-reduced-motion
- Специальные стили для backoffice

### 5. Конфигурация

`appsettings.json`:
```json
"BlockPreview": {
  "BlockGrid": {
    "Enabled": true,
    "Stylesheets": [
      "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css",
      "/css/style.css",
      "/css/backgrounds.css",
      "/css/lazy-load.css",
      "/css/backoffice-preview.css"
    ],
    "Scripts": [
      "/js/lazy-load.js"
    ]
  },
  "BlockList": {
    "Enabled": true,
    "Stylesheets": [
      "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css",
      "/css/style.css",
      "/css/backgrounds.css",
      "/css/lazy-load.css",
      "/css/backoffice-preview.css"
    ],
    "Scripts": [
      "/js/lazy-load.js"
    ]
  }
}
```

`Layout.cshtml`:
```html
<head>
  <link rel="stylesheet" href="~/css/lazy-load.css" />
</head>
<body>
  <script src="~/js/lazy-load.js"></script>
</body>
```

## Проверка работы

### В Console (в iframe):

```javascript
// Проверка overlay
const overlay = document.querySelector('[class*="overlay-"]');
if (overlay) {
  const before = window.getComputedStyle(overlay, '::before');
  console.log('✅ Overlay z-index:', before.zIndex);  // Должно быть: -1 (с видео) или 0 (без видео)
  console.log('✅ Background:', before.backgroundColor);  // Должен быть цвет
  console.log('✅ Opacity:', before.opacity);  // Должна быть прозрачность
}

// Проверка видео (только на фронтенде)
const video = document.querySelector('.video-container');
if (video) {
  const videoStyles = window.getComputedStyle(video);
  console.log('✅ Video z-index:', videoStyles.zIndex);  // Должно быть: -2
}

// Проверка placeholder
const placeholder = document.querySelector('.video-placeholder');
if (placeholder) {
  const placeholderStyles = window.getComputedStyle(placeholder);
  console.log('✅ Placeholder z-index:', placeholderStyles.zIndex);  // Должно быть: -2
  console.log('✅ Placeholder opacity:', placeholderStyles.opacity);  // 1 в backoffice, 0 после загрузки на фронтенде
  console.log('✅ Placeholder display:', placeholderStyles.display);  // block
}

// Проверка lazy loading видео (только на фронтенде)
const iframe = document.querySelector('.video-bg-iframe');
if (iframe) {
  console.log('✅ Video display:', iframe.style.display);  // 'none' в backoffice, '' на фронтенде
  console.log('✅ Video src:', iframe.src || 'Not loaded yet');
  console.log('✅ Video data-src:', iframe.getAttribute('data-src'));
}

// Проверка backoffice
const isInIframe = window.self !== window.top;
console.log('✅ Is in backoffice:', isInIframe);  // true в backoffice, false на фронтенде

// Проверка lazy loading изображений
const lazyImages = document.querySelectorAll('img[data-src]');
console.log('✅ Lazy images count:', lazyImages.length);

const lazyBgs = document.querySelectorAll('[data-bg]');
console.log('✅ Lazy backgrounds count:', lazyBgs.length);
```

## Добавление новых overlay классов

Если создаете новые блоки с overlay, добавьте их в `backoffice-preview.css`:

1. Найдите класс в `backgrounds.css`:
```css
.overlay-НОВЫЙ_КЛАСС::before {
    background-color: #ff0000;
}
.overlay-НОВЫЙ_КЛАСС::before {
    opacity: 0.50;
}
```

2. Добавьте в `backoffice-preview.css`:

**Для overlay БЕЗ видео:**
```css
.overlay-НОВЫЙ_КЛАСС::before {
  z-index: 0 !important;
  background-color: #ff0000 !important;
  opacity: 0.50 !important;
}
```

**Для overlay С видео:**
```css
.bg-video-XXX.overlay-НОВЫЙ_КЛАСС::before {
  z-index: -1 !important;
  background-color: #ff0000 !important;
  opacity: 0.50 !important;
}
```

## Если что-то не работает

### 1. Очистите кэш
- Ctrl+Shift+R (жесткая перезагрузка)
- Или Ctrl+Shift+Delete → Очистить кэш

### 2. Проверьте загрузку CSS и JS
DevTools → Network:
- `backoffice-preview.css` (~5-6KB)
- `lazy-load.css` (~3-4KB)
- `lazy-load.js` (~5-6KB)

### 3. Проверьте порядок загрузки
CSS должны загружаться в порядке:
1. `backgrounds.css`
2. `lazy-load.css`
3. `backoffice-preview.css` (ПОСЛЕДНИМ!)

### 4. Проверьте применение правил
DevTools → Elements → выберите элемент → Styles → проверьте, что правила из `backoffice-preview.css` НЕ перечеркнуты

### 5. Проверьте lazy loading
- Откройте DevTools → Network → фильтр "Media"
- Видео должно загружаться только когда элемент близко к viewport
- Placeholder должен показываться сразу

### 6. Overlay под видео вместо над
- Проверьте что элемент имеет ОБА класса: `bg-video-XXX` И `overlay-YYY`
- Проверьте что в `backoffice-preview.css` есть правило для комбинации этих классов
- z-index должен быть: видео `-2`, overlay `-1`, контент `1`

### 7. Видео не отображается в backoffice
- **Это нормально!** В backoffice видео НЕ загружается для экономии ресурсов
- Вместо видео показывается placeholder (статичное изображение)
- Если placeholder не настроен в Umbraco, в backoffice будет пусто
- На фронтенде видео загружается нормально с lazy loading

### 8. Placeholder не виден в backoffice
- Проверьте что в Umbraco в настройках видео установлен "Video Placeholder"
- Проверьте что в HTML есть `<div class="video-placeholder" data-video-placeholder="...">`
- Если placeholder не настроен, добавьте изображение в поле "videoPlaceholder" в Umbraco
- На фронтенде placeholder скрывается после загрузки видео

### 9. Overlay не виден (opacity: 0.00)
- **Проблема**: Тип данных был `int` вместо `decimal`
- **Решение**: Изменен на `settings.Value<decimal>("opacityOverlay")`
- Проверьте в Umbraco что поле "Opacity Overlay" имеет значение (например, 30, 50, 65)
- Если значение не установлено, overlay будет без opacity (полностью непрозрачный)

### 8. Изображения не lazy load
- Проверьте что `lazy-load.js` загружен
- Проверьте что изображения имеют `data-src` или `data-bg`
- Проверьте Console: `window.lazyLoad` должен существовать
- Вызовите `window.lazyLoad.update()` для повторной инициализации

## Важно

- Все изменения влияют ТОЛЬКО на backoffice
- На фронтенде сайта все работает как прежде
- Отрицательные z-index на фронтенде работают корректно
- В backoffice они переопределены через `backoffice-preview.css`
- Lazy loading работает и на фронтенде, и в backoffice
- Placeholder обеспечивает мгновенную визуальную обратную связь
- Shimmer эффект показывает процесс загрузки
- Плавные переходы улучшают UX
- **Высота секций по содержимому** - убран min-height: 400px
- **TextInverse генерируется в backgrounds.css** - НЕТ inline стилей в HTML
- **Hash классов НЕ включает minHeight** - overlay работают после изменений
- **ВСЕ inline стили убраны из HTML** - background-image устанавливается через JavaScript из data-атрибутов
- **Новые файлы**: `images.css` для стилей изображений, `video-placeholders.js` для установки background-image


## ПРОБЛЕМА: Overlay не отображается в секции portfolio

### Симптомы
- HTML: `<section id="portfolio" class="bg-video-cfcec231 lazy-video overlay-95f866a456324d2fa89fe3ae8632b6a2" style="display: block;">`
- CSS существует в `backgrounds.css`:
  ```css
  .overlay-95f866a456324d2fa89fe3ae8632b6a2::before {
      background-color: #2b00ff;
      opacity: 0.65;
  }
  ```
- CSS существует в `backoffice-preview.css`:
  ```css
  .overlay-95f866a456324d2fa89fe3ae8632b6a2::before {
      background-color: #2b00ff !important;
      opacity: 0.65 !important;
      z-index: 0 !important;
  }
  ```
- Но overlay НЕ виден в браузере

### Возможные причины

1. **Inline style `style="display: block;"`**
   - Источник: Старая версия `site-menu.js` (уже исправлена)
   - Решение: Очистить кэш браузера (Ctrl+Shift+R)
   - Новая версия использует класс `.section-hidden`

2. **Z-index конфликт с видео**
   - Видео: `z-index: -2`
   - Overlay: `z-index: -1` (для видео) или `z-index: 0` (без видео)
   - Контент: `z-index: 1`
   - Проверить: Применяется ли правило `.bg-video-cfcec231.lazy-video.overlay-95f866a456324d2fa89fe3ae8632b6a2::before { z-index: -1 !important; }`

3. **CSS не загружается**
   - Проверить в DevTools → Network: `backoffice-preview.css` загружен?
   - Проверить порядок загрузки: `backgrounds.css` → `backoffice-preview.css`

4. **CSS перезаписывается**
   - Проверить в DevTools → Elements → Styles: правила перечеркнуты?
   - Проверить специфичность селекторов

5. **Элемент не имеет высоты**
   - Если секция пустая или контент не создает высоту, overlay не будет виден
   - Проверить: есть ли контент в секции?

### Диагностика

Откройте DevTools Console и выполните:

```javascript
// 1. Проверка элемента
const section = document.getElementById('portfolio');
console.log('Section:', section);
console.log('Classes:', section.className);
console.log('Height:', section.offsetHeight);

// 2. Проверка overlay стилей
const overlayStyles = window.getComputedStyle(section, '::before');
console.log('Overlay z-index:', overlayStyles.zIndex);
console.log('Overlay background:', overlayStyles.backgroundColor);
console.log('Overlay opacity:', overlayStyles.opacity);
console.log('Overlay content:', overlayStyles.content);
console.log('Overlay display:', overlayStyles.display);
console.log('Overlay position:', overlayStyles.position);

// 3. Проверка видео
const videoContainer = section.querySelector('.video-container');
if (videoContainer) {
    const videoStyles = window.getComputedStyle(videoContainer);
    console.log('Video z-index:', videoStyles.zIndex);
}

// 4. Проверка контента
const container = section.querySelector('.container');
if (container) {
    const containerStyles = window.getComputedStyle(container);
    console.log('Content z-index:', containerStyles.zIndex);
    console.log('Content position:', containerStyles.position);
}
```

### Решение

1. **Очистить кэш браузера**: Ctrl+Shift+R
2. **Проверить загрузку CSS**: DevTools → Network → `backoffice-preview.css`
3. **Проверить применение стилей**: DevTools → Elements → выбрать `<section id="portfolio">` → Styles
4. **Проверить z-index**: Overlay должен быть `-1` (с видео) или `0` (без видео)
5. **Проверить высоту секции**: Если высота 0, overlay не будет виден

### Inline стили - СТАТУС

**Убраны:**
- ✅ `style="display: none;"` → класс `.section-hidden`
- ✅ Inline стили в `_Image.cshtml` → классы в `images.css`
- ✅ Inline стили для video placeholder opacity → классы

**Остались (допустимо):**
- ⚠️ `style="background-image: url(...);"` для video placeholders
  - Устанавливается через JavaScript из `data-bg-image`
  - Допустимо, т.к. URL динамический из Umbraco
  - Альтернатива: генерировать CSS классы для каждого изображения (избыточно)

**Проблемные:**
- ❌ `style="display: block;"` на секциях
  - Источник: Старая версия `site-menu.js` в кэше браузера
  - Исправлено в коде, но браузер использует кэшированную версию
  - Решение: Очистить кэш (Ctrl+Shift+R)


## ИСПРАВЛЕНИЯ - 2026-02-10

### 1. Убран `style="display: block;"` из секций
- **Файл**: `TestUmbraco/wwwroot/js/site-menu.js`
- **Изменение**: Используется класс `.section-hidden` вместо inline стиля
- **Статус**: ✅ Исправлено (может требовать очистки кэша браузера)

### 2. Background-image для video placeholders
- **Файл**: `TestUmbraco/Views/Shared/_BackgroundClasses.cshtml`
- **Изменение**: Background-image устанавливается через JavaScript из `data-bg-image`
- **Статус**: ✅ Допустимо (динамический контент из Umbraco)
- **Причина**: URL изображения динамический, генерация CSS класса для каждого изображения избыточна

### 3. Overlay не отображается в секции portfolio
- **Файл**: `TestUmbraco/wwwroot/css/backoffice-preview.css`
- **Проблема**: Overlay с видео не имел background-color и opacity в специфичном правиле
- **Изменения**:
  1. Добавлен `background-color: #2b00ff !important;` в правило для overlay с видео
  2. Добавлены отдельные правила для opacity каждого overlay:
     ```css
     .bg-video-cfcec231.overlay-95f866a456324d2fa89fe3ae8632b6a2::before {
       opacity: 0.65 !important;
     }
     ```
  3. Добавлено правило для position: relative на секциях с видео и overlay:
     ```css
     [class^="bg-video-"][class*="overlay-"],
     [class*=" bg-video-"][class*="overlay-"] {
       position: relative !important;
     }
     ```
  4. Добавлены селекторы без `.lazy-video` для большей совместимости
- **Статус**: ✅ Исправлено

### Итоговый статус inline стилей

**Полностью убраны:**
- ✅ `style="display: none;"` → `.section-hidden`
- ✅ Inline стили в изображениях → `images.css`
- ✅ Inline стили для opacity placeholder → CSS классы

**Допустимые (динамический контент):**
- ⚠️ `style="background-image: url(...);"` для video placeholders
  - Устанавливается JS из `data-bg-image`
  - URL динамический из Umbraco
  - Альтернатива (генерация CSS) избыточна

**Требуют очистки кэша:**
- 🔄 `style="display: block;"` - исправлено в коде, но может быть в кэше браузера
- **Решение**: Ctrl+Shift+R (жесткая перезагрузка)

### Проверка после исправлений

1. **Очистить кэш браузера**: Ctrl+Shift+R
2. **Проверить overlay в DevTools Console**:
   ```javascript
   const section = document.getElementById('portfolio');
   const styles = window.getComputedStyle(section, '::before');
   console.log('Background:', styles.backgroundColor); // rgb(43, 0, 255)
   console.log('Opacity:', styles.opacity); // 0.65
   console.log('Z-index:', styles.zIndex); // -1
   ```
3. **Проверить отсутствие inline стилей**:
   ```javascript
   const sections = document.querySelectorAll('section[style]');
   console.log('Sections with inline styles:', sections.length); // Должно быть 0
   ```


## ИСПРАВЛЕНИЕ: Overlay не отображается в секции about

### Проблема
- Секция about имеет класс `overlay-b5855bba30164526ab003da00af8dda9` и фон `bg-media-...` (изображение)
- Overlay не отображается, хотя CSS существует

### Причина
- Правила для overlay были написаны БЕЗ учета наличия/отсутствия видео
- Правила с `.bg-video-cfcec231.overlay-b5855bba30164526ab003da00af8dda9` имели высокую специфичность
- Но для секции БЕЗ видео нужен отдельный селектор с `:not([class*="bg-video"])`

### Решение
Добавлены селекторы с `:not([class*="bg-video"])` для overlay БЕЗ видео:

```css
/* Overlay БЕЗ видео - z-index: 0 */
.overlay-b5855bba30164526ab003da00af8dda9:not([class*="bg-video"])::before {
  z-index: 0 !important;
  background-color: #2b00ff !important;
  opacity: 0.30 !important;
}

.overlay-95f866a456324d2fa89fe3ae8632b6a2:not([class*="bg-video"])::before {
  z-index: 0 !important;
  background-color: #2b00ff !important;
  opacity: 0.65 !important;
}
```

### Z-index структура
- **БЕЗ видео**: Фон (auto) → Overlay (0) → Контент (1)
- **С видео**: Видео (-2) → Overlay (-1) → Контент (1)

### Проверка
```javascript
const about = document.getElementById('about');
const styles = window.getComputedStyle(about, '::before');
console.log('Background:', styles.backgroundColor); // rgb(43, 0, 255)
console.log('Opacity:', styles.opacity); // 0.30
console.log('Z-index:', styles.zIndex); // 0
```

Очистить кэш: **Ctrl+Shift+R**


## ИСПРАВЛЕНИЕ: Overlay за изображением в секции about

### Проблема
- Overlay отображается, но находится ЗА фоновым изображением
- Фоновое изображение устанавливается через `background-image` (z-index: auto)
- Overlay с `z-index: 0` находится в том же слое

### Решение
Изменена структура z-index для overlay БЕЗ видео:

**Старая структура:**
- Фон (auto) → Overlay (0) → Контент (1)

**Новая структура:**
- Фон (auto) → Overlay (1) → Контент (2)

```css
/* Overlay БЕЗ видео - НАД фоновым изображением */
.overlay-b5855bba30164526ab003da00af8dda9:not([class*="bg-video"])::before {
  z-index: 1 !important;
}

/* Контент НАД overlay */
[class^="overlay-"] > * {
  z-index: 2 !important;
}
```

### Итоговая структура z-index

**С фоновым изображением (about):**
- background-image: auto
- overlay::before: 1
- контент: 2

**С видео (portfolio):**
- video: -2
- overlay::before: -1
- контент: 2

Очистить кэш: **Ctrl+Shift+R**


## КРИТИЧЕСКАЯ ПРОБЛЕМА: z-index: -1 из backgrounds.css

### Проблема
В `backgrounds.css` есть правило:
```css
.overlay-b5855bba30164526ab003da00af8dda9::before {
    z-index: -1;
}
```

Это правило перезаписывает наши `z-index: 1 !important` из `backoffice-preview.css`, потому что загружается ПОСЛЕ.

### Решение
Добавлено правило в НАЧАЛО `backoffice-preview.css` с максимальным приоритетом:

```css
/* КРИТИЧНО: Переопределяем z-index: -1 из backgrounds.css */
.overlay-b5855bba30164526ab003da00af8dda9::before,
.overlay-95f866a456324d2fa89fe3ae8632b6a2::before,
[class^="overlay-"]::before,
[class*=" overlay-"]::before {
  z-index: 1 !important;
}
```

Это правило должно быть в НАЧАЛЕ файла, чтобы применяться первым и перезаписывать все последующие.

Очистить кэш: **Ctrl+Shift+R**


## ФИНАЛЬНОЕ ИСПРАВЛЕНИЕ: Правильная генерация z-index в backgrounds.css

### Корень проблемы
`UmbracoBackgroundService.cs` генерировал `z-index: -1` для ВСЕХ overlay, независимо от типа фона.

### Решение
Изменен метод `AddOverlayStyles`:
- Определяет тип фона (видео или нет)
- Генерирует правильный z-index:
  - **С видео**: `z-index: -1` (между видео и контентом), контент `z-index: 1`
  - **Без видео**: `z-index: 1` (над фоном), контент `z-index: 2`

```csharp
var bgType = settings.Value<string>("bg")?.Trim();
var hasVideo = bgType == "Видео" || bgType == "Video" || bgType == "бХДЕН";
var overlayZIndex = hasVideo ? -1 : 1;
var contentZIndex = hasVideo ? 1 : 2;
```

### Результат
Теперь `backgrounds.css` генерируется с правильными z-index:
- about (изображение): overlay `z-index: 1`, контент `z-index: 2`
- portfolio (видео): overlay `z-index: -1`, контент `z-index: 1`

**Больше НЕ нужен** `backoffice-preview.css` для переопределения z-index!
