/**
 * Установка background-image для video placeholders
 * Это единственный inline стиль который необходим для динамических изображений
 */

(function() {
    'use strict';
    
    function initVideoPlaceholders() {
        // Находим все video-placeholder с data-bg-image
        var placeholders = document.querySelectorAll('.video-placeholder[data-bg-image]');
        
        placeholders.forEach(function(placeholder) {
            var bgImage = placeholder.getAttribute('data-bg-image');
            if (bgImage) {
                // Единственный inline стиль - background-image (динамический контент)
                placeholder.style.backgroundImage = 'url(' + bgImage + ')';
            }
        });
        
        // Находим все video-placeholder-mobile с data-bg-image
        var mobilePlaceholders = document.querySelectorAll('.video-placeholder-mobile[data-bg-image]');
        
        mobilePlaceholders.forEach(function(placeholder) {
            var bgImage = placeholder.getAttribute('data-bg-image');
            if (bgImage) {
                // Единственный inline стиль - background-image (динамический контент)
                placeholder.style.backgroundImage = 'url(' + bgImage + ')';
            }
        });
    }
    
    // Запуск при загрузке DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initVideoPlaceholders);
    } else {
        initVideoPlaceholders();
    }
    
})();
