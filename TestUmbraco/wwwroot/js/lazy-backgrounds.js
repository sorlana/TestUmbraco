// wwwroot/js/lazy-backgrounds.js
// Конфигурация
const LazyConfig = {
    imageClass: 'lazy-image',
    videoClass: 'lazy-video',
    loadedClass: 'bg-loaded',
    errorClass: 'bg-error'
};

class LazyBackgroundManager {
    constructor() {
        this.observer = null;
        this.config = window.lazyBackgroundsConfig || [];
        this.backgroundsMap = this.createBackgroundsMap();
        this.init();
    }

    createBackgroundsMap() {
        const map = new Map();
        
        if (this.config && Array.isArray(this.config)) {
            this.config.forEach(info => {
                const classes = info.ComponentClass.split(' ');
                const mainClass = classes[0];
                
                if (mainClass) {
                    map.set(mainClass, info);
                }
            });
        }
        
        return map;
    }

    init() {
        if ('IntersectionObserver' in window) {
            this.observer = new IntersectionObserver(
                (entries) => this.handleIntersection(entries),
                {
                    rootMargin: '200px 0px',
                    threshold: 0.01
                }
            );
            
            this.observeElements();
        } else {
            this.loadAllVisible();
        }
        
        // Уже используем passive listeners
        window.addEventListener('scroll', () => this.handleScroll(), { passive: true });
        window.addEventListener('resize', () => this.handleScroll(), { passive: true });
        
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.observeElements());
        }
    }

    observeElements() {
        const imageElements = document.querySelectorAll(`.${LazyConfig.imageClass}:not(.${LazyConfig.loadedClass})`);
        const videoElements = document.querySelectorAll(`.${LazyConfig.videoClass}:not(.${LazyConfig.loadedClass})`);
        
        [...imageElements, ...videoElements].forEach(element => {
            if (this.observer) {
                this.observer.observe(element);
            }
        });
    }

    handleIntersection(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const element = entry.target;
                
                if (element.classList.contains(LazyConfig.imageClass)) {
                    this.loadImage(element);
                } else if (element.classList.contains(LazyConfig.videoClass)) {
                    this.loadVideo(element);
                }
                
                if (this.observer) {
                    this.observer.unobserve(element);
                }
            }
        });
    }

    loadImage(element) {
        const componentClass = this.findComponentClass(element);
        
        if (!componentClass) {
            console.warn('No component class found for lazy image element');
            return;
        }
        
        const backgroundInfo = this.backgroundsMap.get(componentClass);
        
        if (!backgroundInfo || !backgroundInfo.Url) {
            console.warn(`No background info found for class: ${componentClass}`);
            return;
        }
        
        // Просто добавляем класс loaded - все стили уже в CSS
        element.classList.add(LazyConfig.loadedClass);
    }

    loadVideo(element) {
        const componentClass = this.findComponentClass(element);
        
        if (!componentClass) {
            console.warn('No component class found for lazy video element');
            return;
        }
        
        const backgroundInfo = this.backgroundsMap.get(componentClass);
        
        if (!backgroundInfo || !backgroundInfo.VideoId) {
            console.warn(`No video info found for class: ${componentClass}`);
            return;
        }
        
        const isMobile = this.isMobileDevice();
        
        if (isMobile && backgroundInfo.UsePlaceholder && backgroundInfo.PlaceholderUrl) {
            // Для мобильных используем placeholder
            this.createPlaceholder(element, backgroundInfo.PlaceholderUrl);
            element.classList.add(LazyConfig.loadedClass);
        } else {
            // Создаем iframe для видео
            this.createVimeoIframe(element, backgroundInfo.VideoId);
            element.classList.add(LazyConfig.loadedClass);
        }
    }

    addOverlayElement(element) {
        // Проверяем, не добавлен ли уже оверлей
        if (element.querySelector('.background-overlay')) {
            return;
        }
        
        // Создаем элемент оверлея
        const overlay = document.createElement('div');
        overlay.className = 'background-overlay';
        
        // Добавляем оверлей в начало элемента
        if (element.firstChild) {
            element.insertBefore(overlay, element.firstChild);
        } else {
            element.appendChild(overlay);
        }
    }

    createVimeoIframe(element, videoId) {
        // Создаем контейнер для iframe
        const container = document.createElement('div');
        container.className = 'video-container';
        
        // Создаем iframe для Vimeo - исправляем атрибуты allow
        const iframe = document.createElement('iframe');
        iframe.className = 'video-bg-iframe';
        
        // Используем правильные параметры для Vimeo фонового видео
        // Не используем одновременно allow и allowfullscreen, чтобы избежать предупреждения
        iframe.src = `https://player.vimeo.com/video/${videoId}?background=1&muted=1&loop=1&autopause=0&controls=0&title=0&byline=0&portrait=0`;
        iframe.setAttribute('allow', 'autoplay; fullscreen; picture-in-picture');
        iframe.setAttribute('frameborder', '0');
        iframe.setAttribute('scrolling', 'no');
        iframe.setAttribute('title', 'Background video');
        iframe.setAttribute('aria-label', 'Background video');
        
        // Вместо allowfullscreen используем только fullscreen в allow
        // Это предотвратит предупреждение "Allow attribute will take precedence over 'allowfullscreen'"
        
        container.appendChild(iframe);
        
        // Добавляем элемент оверлея если есть класс with-overlay
        if (element.classList.contains('with-overlay')) {
            this.addOverlayElement(element);
        }
        
        // Добавляем контейнер в начало элемента (перед оверлеем, если он есть)
        if (element.firstChild) {
            element.insertBefore(container, element.firstChild);
        } else {
            element.appendChild(container);
        }
    }

    createPlaceholder(element, placeholderUrl) {
        // Создаем контейнер для placeholder
        const container = document.createElement('div');
        container.className = 'video-placeholder-container';
        
        // Создаем изображение placeholder
        const img = document.createElement('img');
        img.className = 'video-placeholder';
        img.src = placeholderUrl;
        img.alt = 'Video placeholder';
        
        container.appendChild(img);
        
        // Добавляем элемент оверлея если есть класс with-overlay
        if (element.classList.contains('with-overlay')) {
            this.addOverlayElement(element);
        }
        
        // Добавляем контейнер в начало элемента
        if (element.firstChild) {
            element.insertBefore(container, element.firstChild);
        } else {
            element.appendChild(container);
        }
    }

    findComponentClass(element) {
        const classList = Array.from(element.classList);
        
        for (const className of classList) {
            if (this.backgroundsMap.has(className)) {
                return className;
            }
        }
        
        return null;
    }

    isMobileDevice() {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
    }

    handleScroll() {
        if (!this.observer) {
            this.loadAllVisible();
        }
    }

    loadAllVisible() {
        const elements = document.querySelectorAll(`.${LazyConfig.imageClass}:not(.${LazyConfig.loadedClass}), .${LazyConfig.videoClass}:not(.${LazyConfig.loadedClass})`);
        
        elements.forEach(element => {
            if (this.isElementInViewport(element)) {
                if (element.classList.contains(LazyConfig.imageClass)) {
                    this.loadImage(element);
                } else if (element.classList.contains(LazyConfig.videoClass)) {
                    this.loadVideo(element);
                }
                
                // Добавляем элемент оверлея если есть класс with-overlay
                if (element.classList.contains('with-overlay')) {
                    this.addOverlayElement(element);
                }
            }
        });
    }

    isElementInViewport(el) {
        const rect = el.getBoundingClientRect();
        return (
            rect.top <= (window.innerHeight || document.documentElement.clientHeight) &&
            rect.bottom >= 0 &&
            rect.left <= (window.innerWidth || document.documentElement.clientWidth) &&
            rect.right >= 0
        );
    }
}

// Инициализация
(function() {
    function init() {
        if (window.lazyBackgroundsConfig && window.lazyBackgroundsConfig.length > 0) {
            window.lazyBackgroundManager = new LazyBackgroundManager();
        }
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        setTimeout(init, 100);
    }
})();