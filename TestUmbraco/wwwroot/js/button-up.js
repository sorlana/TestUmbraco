// Функция для инициализации кнопки "Наверх"
function initButtonUp() {
    var button = document.getElementById('button-up');
    
    if (!button) {
        // Кнопка не найдена (либо showButtonUp = false, либо ещё не создана)
        return;
    }
    
    // Скрываем кнопку по умолчанию
    button.style.display = 'none';
    
    // Обработчик прокрутки
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
    
    // Обработчик клика (с использованием jQuery для анимации, так как он уже подключен)
    button.addEventListener('click', function() {
        if (typeof jQuery !== 'undefined') {
            jQuery('html, body').animate({
                scrollTop: 0
            }, 800);
        } else {
            // Fallback на нативный smooth scroll если jQuery не доступен
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        }
    });
    
    // Инициализация при загрузке
    if (window.pageYOffset > 300) {
        button.style.display = 'block';
        button.style.opacity = '1';
    }
}

// Инициализация после загрузки DOM
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initButtonUp);
} else {
    initButtonUp();
}