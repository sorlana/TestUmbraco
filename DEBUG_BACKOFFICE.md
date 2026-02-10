# Отладка Block Preview - ФИНАЛЬНАЯ ВЕРСИЯ

## ✅ Решение работает

### Что работает
- ✅ Цветные фоны отображаются
- ✅ Градиентные фоны работают
- ✅ Медиа фоны (изображения) видны
- ✅ Overlay отображаются с правильным цветом и прозрачностью
- ✅ Overlay НАД видео фоном (между видео и контентом)
- ✅ `backoffice-preview.css` загружается и применяется
- ✅ Lazy loading видео с placeholder для быстрой загрузки
- ✅ Плавный переход от placeholder к видео
- ✅ Lazy loading для фоновых изображений с shimmer эффектом
- ✅ Lazy loading для обычных изображений с плавным появлением

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
