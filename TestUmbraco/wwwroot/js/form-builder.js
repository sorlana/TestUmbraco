// Client-side валидация и обработка отправки формы
(function() {
    'use strict';
    
    // Инициализация всех форм на странице
    document.addEventListener('DOMContentLoaded', function() {
        console.log('Form builder script loaded');
        const forms = document.querySelectorAll('.form-builder');
        console.log('Found forms:', forms.length);
        forms.forEach(form => initializeForm(form));
    });
    
    function initializeForm(form) {
        console.log('Initializing form:', form.id);
        
        form.addEventListener('submit', async function(e) {
            console.log('Form submit event triggered');
            e.preventDefault();
            e.stopPropagation();
            
            // Скрыть предыдущие сообщения
            hideFormMessage(form);
            
            // Client-side валидация
            if (!validateForm(form)) {
                console.log('Form validation failed');
                // Показать сообщение об ошибке валидации
                const errorFields = form.querySelectorAll('.is-invalid');
                const errorMessages = Array.from(errorFields).map(field => {
                    const formGroup = field.closest('.form-group');
                    const label = formGroup ? formGroup.querySelector('label') : null;
                    const labelText = label ? label.textContent.replace('*', '').trim() : 'Поле';
                    return labelText;
                });
                
                const message = errorMessages.length > 0 
                    ? `Пожалуйста, заполните корректно следующие поля: ${errorMessages.join(', ')}`
                    : 'Пожалуйста, проверьте правильность заполнения полей';
                
                showFormMessage(form, message, 'error');
                return false;
            }
            
            console.log('Form validation passed');
            
            // Получение reCAPTCHA токена
            const formId = form.closest('.form-builder-block').dataset.formId;
            try {
                const token = await getReCaptchaToken();
                document.getElementById(`recaptcha-token-${formId}`).value = token;
                console.log('reCAPTCHA token obtained');
            } catch (error) {
                console.error('reCAPTCHA error:', error);
                showFormMessage(form, 'Ошибка проверки reCAPTCHA. Попробуйте еще раз.', 'error');
                return false;
            }
            
            // Отправка формы через AJAX
            await submitForm(form);
            return false;
        }, true); // Use capture phase
        
        // Real-time валидация полей
        const fields = form.querySelectorAll('input, textarea, select');
        fields.forEach(field => {
            field.addEventListener('blur', () => validateField(field));
            field.addEventListener('input', () => clearFieldError(field));
        });
    }
    
    function validateForm(form) {
        let isValid = true;
        const fields = form.querySelectorAll('[data-required="true"]');
        
        fields.forEach(field => {
            if (!validateField(field)) {
                isValid = false;
            }
        });
        
        return isValid;
    }
    
    function validateField(field) {
        const isRequired = field.dataset.required === 'true';
        const pattern = field.dataset.pattern;
        const errorMessage = field.dataset.error || 'Поле заполнено некорректно';
        const value = field.value.trim();
        
        // Проверка обязательности
        if (isRequired && !value) {
            showFieldError(field, 'Это поле обязательно для заполнения');
            return false;
        }
        
        // Проверка паттерна (только если паттерн не пустой)
        if (pattern && pattern.trim() && value) {
            try {
                const regex = new RegExp(pattern);
                if (!regex.test(value)) {
                    showFieldError(field, errorMessage);
                    return false;
                }
            } catch (e) {
                // Логирование ошибки при некорректном регулярном выражении
                console.error('Invalid regex pattern for field:', field.name || field.id, 'Pattern:', pattern, 'Error:', e);
                // Пропускаем кастомную валидацию при ошибке и продолжаем со стандартной валидацией
            }
        }
        
        // Проверка email
        if (field.type === 'email' && value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(value)) {
                showFieldError(field, 'Введите корректный email адрес');
                return false;
            }
        }
        
        // Проверка телефона
        if (field.type === 'tel' && value) {
            const phoneRegex = /^[\d\s\+\-\(\)]+$/;
            if (!phoneRegex.test(value)) {
                showFieldError(field, 'Введите корректный номер телефона');
                return false;
            }
        }
        
        clearFieldError(field);
        return true;
    }
    
    function showFieldError(field, message) {
        const formGroup = field.closest('.form-group');
        if (!formGroup) return;
        
        const errorSpan = formGroup.querySelector('.field-error');
        if (errorSpan) {
            errorSpan.textContent = message;
            errorSpan.style.display = 'block';
        }
        field.classList.add('is-invalid');
    }
    
    function clearFieldError(field) {
        const formGroup = field.closest('.form-group');
        if (!formGroup) return;
        
        const errorSpan = formGroup.querySelector('.field-error');
        if (errorSpan) {
            errorSpan.style.display = 'none';
        }
        field.classList.remove('is-invalid');
    }
    
    function showFormMessage(form, message, type) {
        const formId = form.closest('.form-builder-block').dataset.formId;
        const messageDiv = document.getElementById(`form-message-${formId}`);
        
        console.log('Showing form message:', message, type, 'messageDiv:', messageDiv);
        
        if (!messageDiv) {
            console.error('Message div not found for form:', formId);
            return;
        }
        
        messageDiv.textContent = message;
        messageDiv.style.display = 'block';
        
        if (type === 'success') {
            messageDiv.style.backgroundColor = '#d4edda';
            messageDiv.style.color = '#155724';
            messageDiv.style.border = '1px solid #c3e6cb';
        } else {
            messageDiv.style.backgroundColor = '#f8d7da';
            messageDiv.style.color = '#721c24';
            messageDiv.style.border = '1px solid #f5c6cb';
        }
        
        // Прокрутка к сообщению (плавная, но не прокручивать всю страницу)
        setTimeout(() => {
            messageDiv.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'nearest' });
        }, 100);
    }
    
    function hideFormMessage(form) {
        const formId = form.closest('.form-builder-block').dataset.formId;
        const messageDiv = document.getElementById(`form-message-${formId}`);
        
        if (messageDiv) {
            messageDiv.style.display = 'none';
        }
    }
    
    async function getReCaptchaToken() {
        return new Promise((resolve, reject) => {
            if (typeof grecaptcha === 'undefined') {
                reject(new Error('reCAPTCHA not loaded'));
                return;
            }
            
            grecaptcha.ready(() => {
                const siteKey = document.querySelector('.g-recaptcha').dataset.sitekey;
                grecaptcha.execute(siteKey, {action: 'submit'})
                    .then(token => resolve(token))
                    .catch(error => reject(error));
            });
        });
    }
    
    async function submitForm(form) {
        const formData = new FormData(form);
        const submitButton = form.querySelector('button[type="submit"]');
        const originalText = submitButton.textContent;
        
        console.log('Submitting form via AJAX');
        
        // Блокировка кнопки
        submitButton.disabled = true;
        submitButton.textContent = 'Отправка...';
        
        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            console.log('Response received:', response.status, response.headers.get('content-type'));
            
            // Проверяем, является ли ответ JSON
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                const result = await response.json();
                console.log('JSON result:', result);
                
                if (result.success) {
                    const successMessage = result.message || formData.get('SuccessMessage') || 'Форма успешно отправлена!';
                    showFormMessage(form, successMessage, 'success');
                    form.reset();
                } else {
                    showFormMessage(form, result.message || 'Произошла ошибка при отправке формы', 'error');
                }
            } else {
                console.log('Non-JSON response, assuming success');
                // Если ответ не JSON (редирект или HTML), считаем успехом
                const successMessage = formData.get('SuccessMessage') || 'Форма успешно отправлена!';
                showFormMessage(form, successMessage, 'success');
                form.reset();
            }
        } catch (error) {
            console.error('Form submission error:', error);
            showFormMessage(form, 'Произошла ошибка при отправке формы. Попробуйте еще раз.', 'error');
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = originalText;
        }
    }
    
    // Экспорт функций для тестирования
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = {
            validateField,
            validateForm,
            showFieldError,
            clearFieldError,
            showFormMessage,
            hideFormMessage
        };
    }
})();
