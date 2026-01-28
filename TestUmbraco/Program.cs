// Program.cs
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы кэширования
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// Добавляем сжатие ответов
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// Настраиваем Umbraco
builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

var app = builder.Build();

// Добавляем HttpContextAccessor (ВАЖНО для работы контроллера)
builder.Services.AddHttpContextAccessor();

// Используем сжатие ответов
app.UseResponseCompression();

// Используем кэширование ответов
app.UseResponseCaching();

// Добавляем middleware для кэширования
app.Use(async (context, next) =>
{
    // Для маршрутов с CSS добавляем заголовки кэширования
    if (context.Request.Path.StartsWithSegments("/api/background"))
    {
        context.Response.GetTypedHeaders().CacheControl =
            new Microsoft.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromHours(1)
            };
        
        context.Response.Headers.Append("Vary", "Accept-Encoding");
    }
    
    await next();
});

await app.BootUmbracoAsync();

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

app.Run();