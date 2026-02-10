using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.ResponseCompression;
using TestUmbraco.Application;
using TestUmbraco.Domain;
using reCAPTCHA.AspNetCore;
using TestUmbraco.Services;
using TestUmbraco.Helpers;
using TestUmbraco.Models.Configuration;
using Umbraco.Community.BlockPreview.Extensions; // Добавьте эту строку

var builder = WebApplication.CreateBuilder(args);

// 1. Сначала HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// 2. Регистрация конфигурации ImageOptimizationSettings
builder.Services.Configure<ImageOptimizationSettings>(
    builder.Configuration.GetSection("ImageOptimization"));

// 3. Наши сервисы регистрируются через Composers
// Не регистрируем здесь, чтобы избежать дублирования

// 4. Другие сервисы
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

// 5. Домен и сервисы из Startup
builder.Services.AddDomain(builder.Configuration);
builder.Services.AddServices();


// 7. Umbraco
builder.Services.AddUmbraco(builder.Environment, builder.Configuration)
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

// 8. Дополнительные сервисы
builder.Services.AddRecaptcha(builder.Configuration.GetSection("RecaptchaSettings"));
builder.Services.AddNotyf(config => 
{
    config.DurationInSeconds = 10;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
});

// 9. Настройка кэширования статических файлов
builder.Services.Configure<StaticFileOptions>(options =>
{
    options.OnPrepareResponse = ctx =>
    {
        // Кэшируем CSS файлы на 7 дней
        var path = ctx.File.Name;
        if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.Append(
                "Cache-Control",
                "public, max-age=604800"); // 7 дней
        }
    };
});

var app = builder.Build();

await app.BootUmbracoAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseResponseCompression();
    app.UseResponseCaching();
}

// Настройка кэширования для backgrounds.css
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name;
        if (path.Contains("backgrounds.css", StringComparison.OrdinalIgnoreCase))
        {
            // Для backgrounds.css используем "immutable" - файл не меняется, пока не изменится его имя
            ctx.Context.Response.Headers.Append(
                "Cache-Control",
                "public, max-age=604800, immutable");
        }
    }
});

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

app.UseNotyf();

// Очистка кэша при запуске приложения
app.Lifetime.ApplicationStarted.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var mediaCacheService = scope.ServiceProvider.GetService<IMediaCacheService>();
    mediaCacheService?.ClearAllCache();
});

app.Run();