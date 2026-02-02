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
        private readonly ILogger<StaticCssInitializer> _logger;

        public StaticCssInitializer(
            IStaticCssGeneratorService cssGenerator,
            ILogger<StaticCssInitializer> logger)
        {
            _cssGenerator = cssGenerator;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Ждем 5 секунд после запуска приложения
                await Task.Delay(5000, stoppingToken);
                
                if (stoppingToken.IsCancellationRequested) return;
                
                _logger.LogInformation("Initializing static CSS file...");
                
                // Проверяем, существует ли файл
                var cssDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css");
                var cssFile = Path.Combine(cssDir, "backgrounds.css");
                
                if (!File.Exists(cssFile))
                {
                    await _cssGenerator.GenerateBackgroundCssFileAsync();
                    _logger.LogInformation("Static CSS file created successfully");
                }
                else
                {
                    _logger.LogInformation("Static CSS file already exists, skipping initialization");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing static CSS file");
            }
        }
    }
}