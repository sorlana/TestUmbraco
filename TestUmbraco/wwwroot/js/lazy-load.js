/**
 * Universal Lazy Loading для изображений и фоновых изображений
 * Работает на фронтенде и в backoffice
 */

(function() {
    'use strict';
    
    // Проверка поддержки IntersectionObserver
    if (!('IntersectionObserver' in window)) {
        // Fallback: загружаем все сразу
        loadAllImages();
        return;
    }
    
    // Настройки observer
    var config = {
        rootMargin: '50px 0px', // Начинаем загрузку за 50px до появления
        threshold: 0.01
    };
    
    // Создаем observer для обычных изображений
    var imageObserver = new IntersectionObserver(function(entries, observer) {
        entries.forEach(function(entry) {
            if (entry.isIntersecting) {
                var img = entry.target;
                loadImage(img);
                observer.unobserve(img);
            }
        });
    }, config);
    
    // Создаем observer для фоновых изображений
    var bgObserver = new IntersectionObserver(function(entries, observer) {
        entries.forEach(function(entry) {
            if (entry.isIntersecting) {
                var element = entry.target;
                loadBackgroundImage(element);
                observer.unobserve(element);
            }
        });
    }, config);
    
    /**
     * Загрузка обычного изображения
     */
    function loadImage(img) {
        var src = img.getAttribute('data-src');
        var srcset = img.getAttribute('data-srcset');
        
        if (!src && !srcset) return;
        
        // Показываем placeholder если есть
        img.classList.add('lazy-loading');
        
        // Создаем новое изображение для предзагрузки
        var tempImg = new Image();
        
        tempImg.onload = function() {
            if (src) img.src = src;
            if (srcset) img.srcset = srcset;
            
            img.classList.remove('lazy-loading');
            img.classList.add('lazy-loaded');
            
            // Удаляем data-атрибуты
            img.removeAttribute('data-src');
            img.removeAttribute('data-srcset');
        };
        
        tempImg.onerror = function() {
            img.classList.remove('lazy-loading');
            img.classList.add('lazy-error');
        };
        
        // Начинаем загрузку
        if (srcset) {
            tempImg.srcset = srcset;
        }
        if (src) {
            tempImg.src = src;
        }
    }
    
    /**
     * Загрузка фонового изображения
     */
    function loadBackgroundImage(element) {
        var bgUrl = element.getAttribute('data-bg');
        
        if (!bgUrl) return;
        
        element.classList.add('lazy-bg-loading');
        
        // Предзагружаем изображение
        var tempImg = new Image();
        
        tempImg.onload = function() {
            element.style.backgroundImage = 'url(' + bgUrl + ')';
            element.classList.remove('lazy-bg-loading');
            element.classList.add('lazy-bg-loaded');
            element.removeAttribute('data-bg');
        };
        
        tempImg.onerror = function() {
            element.classList.remove('lazy-bg-loading');
            element.classList.add('lazy-bg-error');
        };
        
        tempImg.src = bgUrl;
    }
    
    /**
     * Fallback: загрузка всех изображений сразу
     */
    function loadAllImages() {
        // Обычные изображения
        var lazyImages = document.querySelectorAll('img[data-src], img[data-srcset]');
        lazyImages.forEach(function(img) {
            loadImage(img);
        });
        
        // Фоновые изображения
        var lazyBgs = document.querySelectorAll('[data-bg]');
        lazyBgs.forEach(function(element) {
            loadBackgroundImage(element);
        });
    }
    
    /**
     * Инициализация lazy loading
     */
    function init() {
        // Находим все изображения с data-src или data-srcset
        var lazyImages = document.querySelectorAll('img[data-src], img[data-srcset]');
        lazyImages.forEach(function(img) {
            imageObserver.observe(img);
        });
        
        // Находим все элементы с data-bg
        var lazyBgs = document.querySelectorAll('[data-bg]');
        lazyBgs.forEach(function(element) {
            bgObserver.observe(element);
        });
    }
    
    /**
     * Запуск при загрузке DOM
     */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
    
    /**
     * Повторная инициализация для динамически добавленных элементов
     * Можно вызвать вручную: window.lazyLoad.update()
     */
    window.lazyLoad = {
        update: init
    };
    
})();
