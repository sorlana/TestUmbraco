using Microsoft.AspNetCore.ResponseCompression;
using TestUmbraco.Services;
using TestUmbraco.Helpers;
using reCAPTCHA.AspNetCore;
using AspNetCoreHero.ToastNotification;

var builder = WebApplication.CreateBuilder(args);

// HttpContextAccessor ДО Umbraco
builder.Services.AddHttpContextAccessor();

// ✅ РЕГИСТРАЦИЯ НАШИХ СЕРВИСОВ
builder.Services.AddScoped<IMediaCacheService, MediaCacheService>();
builder.Services.AddScoped<ImageHelper>();
builder.Services.AddScoped<IUmbracoBackgroundService, UmbracoBackgroundService>();
// Регистрация статического CSS сервиса
builder.Services.AddSingleton<IStaticCssGeneratorService, StaticCssGeneratorService>();

// Сервисы кэширования
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// Сжатие ответов
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// ✅ РЕГИСТРАЦИЯ UMBRACO 17.0.0 - БЕЗ ПРИСВОЕНИЯ ПЕРЕМЕННОЙ
builder.Services.AddUmbraco(builder.Environment, builder.Configuration)
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

// ✅ ДОПОЛНИТЕЛЬНЫЕ СЕРВИСЫ
builder.Services.AddRecaptcha(builder.Configuration.GetSection("RecaptchaSettings"));
builder.Services.AddNotyf(config => 
{
    config.DurationInSeconds = 10;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
});

var app = builder.Build();

// ЗАГРУЗКА UMBRACO
await app.BootUmbracoAsync();

// Middleware в правильном порядке
app.UseResponseCompression();
app.UseResponseCaching();

// ОСНОВНОЕ MIDDLEWARE UMBRACO
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

// Отладочный middleware для API
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path != null && path.Contains("/umbraco/management/api"))
    {
        app.Logger.LogInformation("🔍 Запрос к API: {Path}", path);
    }
    await next();
});

// Кастомное кэширование ПОСЛЕ Umbraco
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/background"))
    {
        context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromHours(1)
        };
        context.Response.Headers.Append("Vary", "Accept-Encoding");
    }
    await next();
});

// Отладочный эндпоинт
app.MapGet("/debug/info", () =>
{
    return Results.Json(new { status = "ok", message = "Debug endpoint working" });
});

app.Run();