using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.ResponseCompression;
using TestUmbraco.Application;
using TestUmbraco.Domain;
using reCAPTCHA.AspNetCore;
using TestUmbraco.Services;
using TestUmbraco.Helpers;

var builder = WebApplication.CreateBuilder(args);

// 1. Сначала HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// 2. Наши сервисы (ДО Umbraco)
builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddScoped<IMediaCacheService, MediaCacheService>();
builder.Services.AddScoped<ImageHelper>();

// 3. Другие сервисы
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression();

// 4. Домен и сервисы из Startup
builder.Services.AddDomain(builder.Configuration);
builder.Services.AddServices();

// 5. Umbraco
builder.Services.AddUmbraco(builder.Environment, builder.Configuration)
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

// 6. Дополнительные сервисы
builder.Services.AddRecaptcha(builder.Configuration.GetSection("RecaptchaSettings"));
builder.Services.AddNotyf(config => 
{
    config.DurationInSeconds = 10;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
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
}

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

app.Run();