using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using TestUmbraco.Application;
using TestUmbraco.Domain;
using reCAPTCHA.AspNetCore;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http.Extensions;
using TestUmbraco.Services;
using Umbraco.Cms.Core.Services; // Добавлен using для IMediaService

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

            var umbracoBuilder = services.AddUmbraco(_env, _config);
            umbracoBuilder
                .AddBackOffice()
                .AddWebsite()
                .AddComposers()
                .Build();

            services.AddRecaptcha(_config.GetSection("RecaptchaSettings"));

            services.AddNotyf(config => {
                config.DurationInSeconds = 10;
                config.IsDismissable = true;
                config.Position = NotyfPosition.BottomRight; 
            });

            // Регистрация сервиса для работы с фонами
            services.AddScoped<IUmbracoBackgroundService, UmbracoBackgroundService>();
            
            // Добавляем HttpContextAccessor (нужен для фонового сервиса)
            services.AddHttpContextAccessor();

            // Добавляем контроллеры для диагностики
            services.AddControllers();

            // Добавляем кэширование в памяти
            services.AddMemoryCache();
            
            // Добавляем сжатие ответов
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
                
                // В режиме разработки минимальное кэширование
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        var path = ctx.Context.Request.Path.Value ?? "";
                        
                        // Для медиа-файлов в разработке кэшируем на 1 минуту
                        if (path.Contains("/umbraco/api/media/get"))
                        {
                            ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=60";
                            ctx.Context.Response.Headers["X-Cache-Mode"] = "development";
                        }
                    }
                });
            }
            else
            {
                // В продакшн режиме включаем сжатие
                app.UseResponseCompression();
                
                // Настраиваем статические файлы с умным кэшированием
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        var path = ctx.Context.Request.Path.Value ?? "";
                        
                        // Кэширование медиа-файлов с версией
                        if (path.Contains("/umbraco/api/media/get"))
                        {
                            // Проверяем, есть ли параметр версии
                            if (ctx.Context.Request.Query.ContainsKey("v"))
                            {
                                var version = ctx.Context.Request.Query["v"].ToString();
                                
                                // Если есть версия, кэшируем на год
                                ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000";
                                ctx.Context.Response.Headers["ETag"] = $"\"{version}\"";
                                ctx.Context.Response.Headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
                            }
                            else
                            {
                                // Если нет версии, кэшируем на 1 день
                                ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=86400";
                            }
                        }
                        else if (path.EndsWith(".css") || path.EndsWith(".js"))
                        {
                            // CSS и JS кэшируем на 1 неделю
                            ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=604800";
                        }
                        else if (path.EndsWith(".jpg") || path.EndsWith(".jpeg") || 
                                 path.EndsWith(".png") || path.EndsWith(".gif") || 
                                 path.EndsWith(".webp") || path.EndsWith(".svg"))
                        {
                            // Изображения без версии кэшируем на 1 месяц
                            ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=2592000";
                        }
                    }
                });
            }

            // Добавляем диагностические endpoints перед Umbraco
            AddDiagnosticEndpoints(app, env);

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

        private void AddDiagnosticEndpoints(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Endpoint для проверки путей шаблонов
            app.Map("/debug/templates", diagnosticApp =>
            {
                diagnosticApp.Run(async context =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    var contentRoot = env.ContentRootPath;
                    var webRoot = env.WebRootPath;
                    var viewsPath = Path.Combine(contentRoot, "Views");
                    
                    logger.LogInformation("Диагностика путей шаблонов...");

                    var results = new
                    {
                        Timestamp = DateTime.Now,
                        Environment = env.EnvironmentName,
                        Paths = new
                        {
                            ContentRoot = contentRoot,
                            WebRoot = webRoot,
                            Views = viewsPath,
                            ViewsExists = Directory.Exists(viewsPath)
                        },
                        Templates = new List<object>()
                    };

                    // Проверяем файлы шаблонов
                    if (Directory.Exists(viewsPath))
                    {
                        var files = Directory.GetFiles(viewsPath, "*.cshtml", SearchOption.AllDirectories)
                            .Select(f => new
                            {
                                Path = f,
                                Name = Path.GetFileName(f),
                                RelativePath = f.Replace(contentRoot, "~").Replace("\\", "/"),
                                Size = new FileInfo(f).Length,
                                LastModified = File.GetLastWriteTime(f)
                            });

                        results.Templates.AddRange(files);
                    }

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(results);
                });
            });

            // Endpoint для проверки кэширования
            app.Map("/debug/cache-info", diagnosticApp =>
            {
                diagnosticApp.Run(async context =>
                {
                    var httpContext = context;
                    var cacheControl = httpContext.Response.Headers["Cache-Control"].ToString();
                    var expires = httpContext.Response.Headers["Expires"].ToString();
                    var etag = httpContext.Response.Headers["ETag"].ToString();
                    
                    var result = new
                    {
                        RequestPath = httpContext.Request.Path,
                        RequestQuery = httpContext.Request.QueryString,
                        CacheControl = cacheControl,
                        Expires = expires,
                        ETag = etag,
                        IsMediaRequest = httpContext.Request.Path.Value?.Contains("/umbraco/api/media/get") == true
                    };

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(result);
                });
            });

            // Endpoint для проверки устройства
            app.Map("/debug/device-info", diagnosticApp =>
            {
                diagnosticApp.Run(async context =>
                {
                    var userAgent = context.Request.Headers["User-Agent"].ToString();
                    var isMobile = userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
                                   userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) ||
                                   userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase);
                    
                    var result = new
                    {
                        UserAgent = userAgent,
                        IsMobile = isMobile,
                        Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
                    };

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(result);
                });
            });

            // Endpoint для быстрой проверки работы приложения
            app.Map("/health", healthApp =>
            {
                healthApp.Run(async context =>
                {
                    var result = new
                    {
                        Status = "Running",
                        Time = DateTime.Now,
                        Environment = env.EnvironmentName,
                        Machine = Environment.MachineName,
                        ViewsFolder = Path.Combine(env.ContentRootPath, "Views"),
                        ViewsExists = Directory.Exists(Path.Combine(env.ContentRootPath, "Views"))
                    };
                    
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(result);
                });
            });
            
            // Endpoint для проверки версии медиафайла
            app.Map("/debug/media-version/{mediaKey:guid}", diagnosticApp =>
            {
                diagnosticApp.Run(async context =>
                {
                    var mediaKey = Guid.Parse(context.Request.RouteValues["mediaKey"]!.ToString()!);
                    var mediaService = context.RequestServices.GetRequiredService<IMediaService>();
                    var media = mediaService.GetById(mediaKey);
                    
                    if (media == null)
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("Media not found");
                        return;
                    }
                    
                    var umbracoFile = media.GetValue<string>("umbracoFile");
                    var updateDate = media.UpdateDate;
                    var createDate = media.CreateDate;
                    
                    var result = new
                    {
                        MediaKey = mediaKey,
                        Name = media.Name,
                        UmbracoFile = umbracoFile,
                        UpdateDate = updateDate,
                        CreateDate = createDate,
                        UmbracoVersion = $"{updateDate.Ticks}"
                    };
                    
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(result);
                });
            });
        }
    }
}