using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using TestUmbraco.Application;
using TestUmbraco.Domain;
using reCAPTCHA.AspNetCore;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using TestUmbraco.Services;
using Umbraco.Cms.Core.Services;
using TestUmbraco.Helpers;

namespace TestUmbraco
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _env = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDomain(_config);
            services.AddServices();

            // ✅ РЕГИСТРАЦИЯ НАШИХ СЕРВИСОВ
            services.AddScoped<IMediaCacheService, MediaCacheService>();
            services.AddScoped<ImageHelper>();
            services.AddScoped<IUmbracoBackgroundService, UmbracoBackgroundService>();
            services.AddHttpContextAccessor();
            
            // Umbraco регистрация
            services.AddUmbraco(_env, _config)
                .AddBackOffice()
                .AddWebsite()
                .AddComposers()
                .Build();

            // Дополнительные сервисы
            services.AddRecaptcha(_config.GetSection("RecaptchaSettings"));
            services.AddNotyf(config => {
                config.DurationInSeconds = 10;
                config.IsDismissable = true;
                config.Position = NotyfPosition.BottomRight; 
            });

            // Кэширование
            services.AddMemoryCache();
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = new[] 
                {
                    "text/css",
                    "application/javascript",
                    "text/html",
                    "application/json",
                    "image/svg+xml"
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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
        }
    }
}