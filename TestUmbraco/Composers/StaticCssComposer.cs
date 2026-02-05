using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestUmbraco.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace TestUmbraco.Composers
{
    public class StaticCssComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IStaticCssGeneratorService, StaticCssGeneratorService>();
            
            // Запускаем инициализацию CSS при старте приложения
            builder.Services.AddSingleton<IHostedService, StaticCssInitializer>();
        }
    }
    
    public class StaticCssInitializer : BackgroundService
    {
        private readonly IStaticCssGeneratorService _cssGenerator;
        private readonly ILoggingService _loggingService;

        public StaticCssInitializer(
            IStaticCssGeneratorService cssGenerator,
            ILoggingService loggingService)
        {
            _cssGenerator = cssGenerator;
            _loggingService = loggingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Ждем 5 секунд после запуска приложения
                await Task.Delay(5000, stoppingToken);
                
                if (stoppingToken.IsCancellationRequested) return;
                
                _loggingService.LogInformation<StaticCssInitializer>("Initializing static CSS file...");
                
                // Проверяем, существует ли файл
                var cssDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css");
                var cssFile = Path.Combine(cssDir, "backgrounds.css");
                
                if (!File.Exists(cssFile))
                {
                    await _cssGenerator.GenerateBackgroundCssFileAsync();
                    _loggingService.LogInformation<StaticCssInitializer>("Static CSS file created successfully");
                }
                else
                {
                    _loggingService.LogInformation<StaticCssInitializer>("Static CSS file already exists, skipping initialization");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError<StaticCssInitializer>($"Error initializing static CSS file", ex);
            }
        }
    }
}