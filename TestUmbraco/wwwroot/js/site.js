/**
 * =========================================
 * SITE.JS - Объединенный JavaScript файл
 * =========================================
 * Содержит все скрипты сайта в одном файле
 */

// =========================================
// BUTTON UP - Кнопка "Наверх"
// =========================================

(function() {
    'use strict';
    
    function initButtonUp() {
        var button = document.getElementById('button-up');
        
        if (!button) {
            return;
        }
        
        button.style.display = 'none';
        
        window.addEventListener('scroll', function() {
            if (window.pageYOffset > 300) {
                button.style.display = 'block';
                setTimeout(function() {
                    button.style.opacity = '1';
                }, 10);
            } else {
                button.style.opacity = '0';
                setTimeout(function() {
                    if (window.pageYOffset <= 300) {
                        button.style.display = 'none';
                    }
                }, 300);
            }
        });
        
        button.addEventListener('click', function() {
            if (typeof jQuery !== 'undefined') {
                jQuery('html, body').animate({
                    scrollTop: 0
                }, 800);
            } else {
                window.scrollTo({
                    top: 0,
                    behavior: 'smooth'
                });
            }
        });
        
        if (window.pageYOffset > 300) {
            button.style.display = 'block';
            button.style.opacity = '1';
        }
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initButtonUp);
    } else {
        initButtonUp();
    }
})();

// =========================================
// LAZY LOAD - Универсальная ленивая загрузка
// =========================================

(function() {
    'use strict';
    
    if (!('IntersectionObserver' in window)) {
        loadAllImages();
        return;
    }
    
    var config = {
        rootMargin: '50px 0px',
        threshold: 0.01
    };
    
    var imageObserver = new IntersectionObserver(function(entries, observer) {
        entries.forEach(function(entry) {
            if (entry.isIntersecting) {
                var img = entry.target;
                loadImage(img);
                observer.unobserve(img);
            }
        });
    }, config);
    
    var bgObserver = new IntersectionObserver(function(entries, observer) {
        entries.forEach(function(entry) {
            if (entry.isIntersecting) {
                var element = entry.target;
                loadBackgroundImage(element);
                observer.unobserve(element);
            }
        });
    }, config);
    
    function loadImage(img) {
        var src = img.getAttribute('data-src');
        var srcset = img.getAttribute('data-srcset');
        
        if (!src && !srcset) return;
        
        img.classList.add('lazy-loading');
        
        var tempImg = new Image();
        
        tempImg.onload = function() {
            if (src) img.src = src;
            if (srcset) img.srcset = srcset;
            
            img.classList.remove('lazy-loading');
            img.classList.add('lazy-loaded');
            
            img.removeAttribute('data-src');
            img.removeAttribute('data-srcset');
        };
        
        tempImg.onerror = function() {
            img.classList.remove('lazy-loading');
            img.classList.add('lazy-error');
        };
        
        if (srcset) {
            tempImg.srcset = srcset;
        }
        if (src) {
            tempImg.src = src;
        }
    }
    
    function loadBackgroundImage(element) {
        var bgUrl = element.getAttribute('data-bg');
        
        if (!bgUrl) return;
        
        element.classList.add('lazy-bg-loading');
        
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
    
    function loadAllImages() {
        var lazyImages = document.querySelectorAll('img[data-src], img[data-srcset]');
        lazyImages.forEach(function(img) {
            loadImage(img);
        });
        
        var lazyBgs = document.querySelectorAll('[data-bg]');
        lazyBgs.forEach(function(element) {
            loadBackgroundImage(element);
        });
    }
    
    function init() {
        var lazyImages = document.querySelectorAll('img[data-src], img[data-srcset]');
        lazyImages.forEach(function(img) {
            imageObserver.observe(img);
        });
        
        var lazyBgs = document.querySelectorAll('[data-bg]');
        lazyBgs.forEach(function(element) {
            bgObserver.observe(element);
        });
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
    
    window.lazyLoad = {
        update: init
    };
    
})();

// =========================================
// VIDEO PLACEHOLDERS - Установка фоновых изображений
// =========================================

(function() {
    'use strict';
    
    function initVideoPlaceholders() {
        var placeholders = document.querySelectorAll('.video-placeholder[data-bg-image]');
        
        placeholders.forEach(function(placeholder) {
            var bgImage = placeholder.getAttribute('data-bg-image');
            if (bgImage) {
                placeholder.style.backgroundImage = 'url(' + bgImage + ')';
            }
        });
        
        var mobilePlaceholders = document.querySelectorAll('.video-placeholder-mobile[data-bg-image]');
        
        mobilePlaceholders.forEach(function(placeholder) {
            var bgImage = placeholder.getAttribute('data-bg-image');
            if (bgImage) {
                placeholder.style.backgroundImage = 'url(' + bgImage + ')';
            }
        });
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initVideoPlaceholders);
    } else {
        initVideoPlaceholders();
    }
    
})();

// =========================================
// SITE MENU - Меню и навигация
// =========================================

(function() {
    'use strict';
    
    function toggleLandingSections() {
        const menuLandingDataElement = document.getElementById('menu-landing-data');
        
        if (!menuLandingDataElement) {
            return;
        }
        
        try {
            const menuLandingData = JSON.parse(menuLandingDataElement.textContent);
            
            menuLandingData.forEach(item => {
                const isHomeSection = item.name?.toLowerCase() === "home" || 
                                      item.id?.toLowerCase() === "home";
                
                if (!isHomeSection) {
                    const section = document.getElementById(item.id);
                    if (section) {
                        if (item.show) {
                            section.classList.remove('section-hidden');
                        } else {
                            section.classList.add('section-hidden');
                        }
                    }
                }
            });
        } catch (error) {
            console.error('Ошибка при обработке данных меню лендинга:', error);
        }
    }
    
    function initMobileMenu() {
        // Проверяем, что jQuery доступен
        if (typeof jQuery === 'undefined') {
            console.warn('jQuery is not available for mobile menu initialization');
            return;
        }
        
        // Используем primer подход с jQuery
        var touch = $('.mobile-menu-toggle');
        var mobileOverlay = $('.mobile-menu-overlay');
        var mobileClose = $('.mobile-menu-close');
        var mobileContainer = $('.mobile-menu-container');
        
        // Обработчик клика по бургеру
        touch.on('click', function(e) {
            e.preventDefault();
            // Переключаем видимость меню
            mobileOverlay.toggleClass('active');
            
            // Управление прокруткой body
            if (mobileOverlay.hasClass('active')) {
                $('body').css('overflow', 'hidden');
            } else {
                $('body').css('overflow', '');
            }
            
            // Добавляем/убираем класс open для анимации бургера
            $(this).toggleClass('open');
        });
        
        // Обработчик изменения размера окна
        $(window).resize(function(){
            var w = $(window).width();
            if (w > 768 && mobileOverlay.hasClass('active')) {
                mobileOverlay.removeClass('active');
                $('body').css('overflow', '');
                touch.removeClass('open');
            }
        });
        
        // Обработчик клика по ссылкам в меню
        $('.nav a').on('click', function(){
            if (touch.css('display') != 'none'){
                touch.trigger("click");
            }
        });
        
        // Обработчик клика по кнопке закрытия
        if (mobileClose.length) {
            mobileClose.on('click', function(e) {
                e.preventDefault();
                mobileOverlay.removeClass('active');
                $('body').css('overflow', '');
                touch.removeClass('open');
            });
        }
        
        // Обработчик клика по оверлею
        mobileOverlay.on('click', function(e) {
            if (e.target === this) {
                $(this).removeClass('active');
                $('body').css('overflow', '');
                touch.removeClass('open');
            }
        });
    }
    
    function initSmoothScroll() {
        document.querySelectorAll('.landing-menu-link, .mobile-landing-link').forEach(anchor => {
            anchor.addEventListener('click', function(e) {
                const href = this.getAttribute('href');
                
                if (href && href.startsWith('#')) {
                    e.preventDefault();
                    
                    const mobileOverlay = document.querySelector('.mobile-menu-overlay');
                    if (mobileOverlay && mobileOverlay.classList.contains('active')) {
                        mobileOverlay.classList.remove('active');
                        document.body.style.overflow = '';
                    }
                    
                    const target = document.querySelector(href);
                    if (target) {
                        if (typeof jQuery !== 'undefined') {
                            jQuery('html, body').animate({
                                scrollTop: jQuery(target).offset().top
                            }, 800);
                        } else {
                            target.scrollIntoView({
                                behavior: 'smooth',
                                block: 'start'
                            });
                        }
                    }
                }
            });
        });
    }
    
    function initSiteMenu() {
        // Добавляем задержку для корректной инициализации
        setTimeout(function() {
            initMobileMenu();
            initSmoothScroll();
            toggleLandingSections();
            document.body.classList.add('site-menu-loaded');
        }, 100);
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSiteMenu);
    } else {
        initSiteMenu();
    }
    
    
    function initScrollHighlight() {
        // Получаем все ссылки меню лендинга
        const menuLinks = document.querySelectorAll('.landing-menu-link, .mobile-landing-link');
        
        // Создаем массив секций и соответствующих ссылок
        const sections = [];
        
        menuLinks.forEach(link => {
            const sectionId = link.getAttribute('href').substring(1);
            const section = document.getElementById(sectionId);
            
            if (section) {
                sections.push({
                    element: section,
                    link: link,
                    id: sectionId
                });
            }
        });
        
        // Функция для определения активной секции
        function setActiveSection() {
            let currentSection = '';
            const scrollPosition = window.scrollY + 100; // Небольшой отступ сверху
            
            sections.forEach(section => {
                const sectionTop = section.element.offsetTop;
                const sectionHeight = section.element.offsetHeight;
                
                if (scrollPosition >= sectionTop && scrollPosition < sectionTop + sectionHeight) {
                    currentSection = section.id;
                }
            });
            
            // Удаляем класс active у всех ссылок
            menuLinks.forEach(link => {
                link.classList.remove('active');
            });
            
            // Добавляем класс active к ссылке текущей секции
            if (currentSection) {
                const activeLink = document.querySelector(`.landing-menu-link[href="#${currentSection}"], .mobile-landing-link[href="#${currentSection}"]`);
                if (activeLink) {
                    activeLink.classList.add('active');
                }
            }
        }
        
        // Добавляем обработчик прокрутки
        window.addEventListener('scroll', setActiveSection);
        
        // Вызываем функцию сразу, чтобы установить активную секцию при загрузке страницы
        setActiveSection();
    }
    
    function initSiteMenu() {
        // Добавляем задержку для корректной инициализации
        setTimeout(function() {
            initMobileMenu();
            initSmoothScroll();
            toggleLandingSections();
            initScrollHighlight(); // Добавляем вызов новой функции
            document.body.classList.add('site-menu-loaded');
        }, 100);
    }
})();

// =========================================
// LAZY BACKGROUNDS - Фоновые изображения и видео
// ВАЖНО: Загружается ПОСЛЕ _BackgroundConfig
// =========================================

(function() {
    'use strict';
    
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
                this.createPlaceholder(element, backgroundInfo.PlaceholderUrl);
                element.classList.add(LazyConfig.loadedClass);
            } else {
                this.createVimeoIframe(element, backgroundInfo.VideoId);
                element.classList.add(LazyConfig.loadedClass);
            }
        }

        addOverlayElement(element) {
            if (element.querySelector('.background-overlay')) {
                return;
            }
            
            const overlay = document.createElement('div');
            overlay.className = 'background-overlay';
            
            if (element.firstChild) {
                element.insertBefore(overlay, element.firstChild);
            } else {
                element.appendChild(overlay);
            }
        }

        createVimeoIframe(element, videoId) {
            const container = document.createElement('div');
            container.className = 'video-container';
            
            const iframe = document.createElement('iframe');
            iframe.className = 'video-bg-iframe';
            iframe.src = `https://player.vimeo.com/video/${videoId}?background=1&muted=1&loop=1&autopause=0&controls=0&title=0&byline=0&portrait=0`;
            iframe.setAttribute('allow', 'autoplay; fullscreen; picture-in-picture');
            iframe.setAttribute('frameborder', '0');
            iframe.setAttribute('scrolling', 'no');
            iframe.setAttribute('title', 'Background video');
            iframe.setAttribute('aria-label', 'Background video');
            
            container.appendChild(iframe);
            
            if (element.classList.contains('with-overlay')) {
                this.addOverlayElement(element);
            }
            
            if (element.firstChild) {
                element.insertBefore(container, element.firstChild);
            } else {
                element.appendChild(container);
            }
        }

        createPlaceholder(element, placeholderUrl) {
            const container = document.createElement('div');
            container.className = 'video-placeholder-container';
            
            const img = document.createElement('img');
            img.className = 'video-placeholder';
            img.src = placeholderUrl;
            img.alt = 'Video placeholder';
            
            container.appendChild(img);
            
            if (element.classList.contains('with-overlay')) {
                this.addOverlayElement(element);
            }
            
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