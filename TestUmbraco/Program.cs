// Program.cs
using Microsoft.AspNetCore.ResponseCompression;
using Umbraco.Cms.Core.DependencyInjection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// HttpContextAccessor ДО Umbraco
builder.Services.AddHttpContextAccessor();

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

// РЕГИСТРАЦИЯ UMBRACO 17.0.0
builder.Services.AddUmbraco(builder.Environment, builder.Configuration)
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers() // ← КРИТИЧЕСКИ ВАЖНО ДЛЯ КАСТОМНЫХ РЕДАКТОРОВ
    .Build();

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
        // Только эти эндпоинты для Umbraco 17.0.0
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

// Отладочный middleware для API (перед кастомным кэшированием)
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

// 👇 ПРОСТОЙ ОТЛАДОЧНЫЙ ЭНДПОИНТ 👇
app.MapGet("/debug/info", () =>
{
    return Results.Json(new { status = "ok", message = "Debug endpoint working" });
});
// 👆 ПРОСТОЙ ОТЛАДОЧНЫЙ ЭНДПОИНТ 👆

app.Run();