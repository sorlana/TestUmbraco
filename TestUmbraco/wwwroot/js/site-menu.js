// Функция для управления видимостью секций лендинга
function toggleLandingSections() {
    const menuLandingDataElement = document.getElementById('menu-landing-data');
    
    if (!menuLandingDataElement) {
        return; // Данных нет, выходим
    }
    
    try {
        const menuLandingData = JSON.parse(menuLandingDataElement.textContent);
        
        menuLandingData.forEach(item => {
            // Пропускаем секцию Home
            const isHomeSection = item.name?.toLowerCase() === "home" || 
                                  item.id?.toLowerCase() === "home";
            
            if (!isHomeSection) {
                const section = document.getElementById(item.id);
                if (section) {
                    section.style.display = item.show ? 'block' : 'none';
                }
            }
        });
    } catch (error) {
        console.error('Ошибка при обработке данных меню лендинга:', error);
    }
}

// Функция для инициализации мобильного меню
function initMobileMenu() {
    const mobileToggle = document.querySelector('.mobile-menu-toggle');
    const mobileClose = document.querySelector('.mobile-menu-close');
    const mobileOverlay = document.querySelector('.mobile-menu-overlay');
    
    if (!mobileToggle || !mobileClose || !mobileOverlay) {
        return; // Элементы не найдены
    }
    
    // Открытие мобильного меню
    mobileToggle.addEventListener('click', function() {
        mobileOverlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    });
    
    // Закрытие мобильного меню
    mobileClose.addEventListener('click', function() {
        mobileOverlay.classList.remove('active');
        document.body.style.overflow = '';
    });
    
    // Закрытие при клике вне меню
    mobileOverlay.addEventListener('click', function(e) {
        if (e.target === mobileOverlay) {
            mobileOverlay.classList.remove('active');
            document.body.style.overflow = '';
        }
    });
}

// Функция для плавного скролла
function initSmoothScroll() {
    document.querySelectorAll('.landing-menu-link, .mobile-landing-link').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            
            if (href && href.startsWith('#')) {
                e.preventDefault();
                
                // Закрываем мобильное меню если открыто
                const mobileOverlay = document.querySelector('.mobile-menu-overlay');
                if (mobileOverlay && mobileOverlay.classList.contains('active')) {
                    mobileOverlay.classList.remove('active');
                    document.body.style.overflow = '';
                }
                
                const target = document.querySelector(href);
                if (target) {
                    // Используем jQuery для анимации, если он доступен
                    if (typeof jQuery !== 'undefined') {
                        jQuery('html, body').animate({
                            scrollTop: jQuery(target).offset().top
                        }, 800);
                    } else {
                        // Нативный smooth scroll
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

// Основная функция инициализации
function initSiteMenu() {
    // Инициализация мобильного меню
    initMobileMenu();
    
    // Инициализация плавного скролла
    initSmoothScroll();
    
    // Управление секциями лендинга
    toggleLandingSections();
    
    // Добавляем класс к body для поддержки меню
    document.body.classList.add('site-menu-loaded');
}

// Инициализация при загрузке DOM
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initSiteMenu);
} else {
    initSiteMenu();
}

// Экспорт для использования в других модулях
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        initSiteMenu,
        initMobileMenu,
        initSmoothScroll,
        toggleLandingSections
    };
}