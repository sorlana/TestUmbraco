// Services/UmbracoBackgroundService.cs
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;
using System.Collections.Generic;

namespace TestUmbraco.Services
{
    public class UmbracoBackgroundService : IUmbracoBackgroundService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UmbracoBackgroundService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public BackgroundResult ProcessBackground(IPublishedElement? settings, Guid componentId, string prefix = "bg")
        {
            var result = new BackgroundResult();
            
            if (settings == null)
            {
                return result;
            }

            // Проверяем, есть ли настройки и выбрано ли значение в bg
            if (settings.HasProperty("bg") && settings.HasValue("bg"))
            {
                var bgValue = settings.Value<string>("bg");
                
                // Если выбрано "Изображение"
                if (bgValue == "Изображение")
                {
                    // Проверяем наличие фонового изображения
                    if (settings.HasProperty("backgroundImage") && settings.HasValue("backgroundImage"))
                    {
                        var bgImage = settings.Value<IPublishedContent>("backgroundImage");
                        if (bgImage != null)
                        {
                            // Создаем уникальный класс для компонента
                            var bgClass = $"{prefix}-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                            
                            // Получаем минимальную высоту (если есть)
                            var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                            
                            // Получаем значение bgSize и преобразуем в CSS-значение
                            var bgSize = "auto"; // значение по умолчанию, если ничего не выбрано
                            if (settings.HasProperty("bgSize") && settings.HasValue("bgSize"))
                            {
                                var bgSizeValue = settings.Value<string>("bgSize");
                                if (!string.IsNullOrWhiteSpace(bgSizeValue))
                                {
                                    bgSize = ConvertBgSizeToCss(bgSizeValue);
                                }
                            }
                            
                            // Позиция фона (если есть)
                            var bgPosition = settings.HasValue("backgroundPosition") 
                                ? settings.Value<string>("backgroundPosition") 
                                : "center";
                            
                            // Формируем параметр для API
                            var backgroundParam = $"{bgImage.Key}:{bgClass}:{minHeight}:{bgSize}:{bgPosition}";
                            
                            // Добавляем в Context.Items
                            AddToContextItems(backgroundParam);
                            
                            result.CssClass = bgClass;
                            result.ApiParam = backgroundParam;
                            result.HasBackground = true;
                        }
                    }
                }
            }
            
            return result;
        }

        private void AddToContextItems(string backgroundParam)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var backgroundParams = httpContext.Items["BackgroundParams"] as List<string>;
            if (backgroundParams == null)
            {
                backgroundParams = new List<string>();
                httpContext.Items["BackgroundParams"] = backgroundParams;
            }
            backgroundParams.Add(backgroundParam);
        }

        private string ConvertBgSizeToCss(string bgSizeValue)
        {
            if (string.IsNullOrWhiteSpace(bgSizeValue))
            {
                return "auto";
            }
            
            return bgSizeValue.Trim() switch
            {
                "Как есть" => "auto",
                "По ширине" => "100% auto",
                "По высоте" => "auto 100%",
                "Обложка" => "cover",
                "Вместить" => "contain",
                _ => "auto"
            };
        }
    }
}