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

            // Добавляем контроллеры для диагностики
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
                    // Убрано: u.UseInstallEndpoints() - в Umbraco 17 установщик включается автоматически
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

            // Endpoint для проверки пути приложения
            app.Map("/debug/paths", diagnosticApp =>
            {
                diagnosticApp.Run(async context =>
                {
                    var currentDir = Directory.GetCurrentDirectory();
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    
                    var result = new
                    {
                        AppDomainBaseDirectory = baseDir,
                        DirectoryGetCurrentDirectory = currentDir,
                        EnvironmentContentRootPath = env.ContentRootPath,
                        EnvironmentWebRootPath = env.WebRootPath,
                        EnvironmentApplicationName = env.ApplicationName,
                        RequestPath = context.Request.Path,
                        RequestQueryString = context.Request.QueryString,
                        RequestUrl = context.Request.GetDisplayUrl()
                    };

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(result);
                });
            });

            // Endpoint для ручной проверки существования файлов
            app.Map("/debug/file-check/{filename?}", diagnosticApp =>
            {
                diagnosticApp.Run(async context =>
                {
                    var filename = context.Request.RouteValues["filename"] as string ?? "BlockGrid.cshtml";
                    var basePath = env.ContentRootPath;
                    
                    var possiblePaths = new[]
                    {
                        Path.Combine(basePath, "Views", filename),
                        Path.Combine(basePath, "Views", "BlockGrid", filename),
                        Path.Combine(basePath, "Views", "BlockGrid", "Index.cshtml"),
                        Path.Combine(basePath, filename)
                    };

                    var checks = new List<object>();
                    
                    foreach (var path in possiblePaths)
                    {
                        var exists = File.Exists(path);
                        checks.Add(new
                        {
                            Path = path,
                            RelativePath = path.Replace(basePath, "~").Replace("\\", "/"),
                            Exists = exists,
                            Details = exists ? new
                            {
                                Size = new FileInfo(path).Length,
                                LastModified = File.GetLastWriteTime(path),
                                IsReadOnly = new FileInfo(path).IsReadOnly
                            } : null
                        });
                    }

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        CheckedFile = filename,
                        BasePath = basePath,
                        Checks = checks
                    });
                });
            });

            // Endpoint для проверки конфигурации Umbraco
            app.Map("/debug/umbraco-config", diagnosticApp =>
            {
                diagnosticApp.Run(async context =>
                {
                    var config = context.RequestServices.GetRequiredService<IConfiguration>();
                    
                    var umbracoConfig = new
                    {
                        TemplateFolders = config.GetSection("Umbraco:CMS:Runtime:TemplateFolders").Get<string[]>(),
                        EnableTemplateFolders = config["Umbraco:CMS:Templates:EnableTemplateFolders"],
                        DefaultRenderingEngine = config["Umbraco:CMS:Templates:DefaultRenderingEngine"],
                        ResolveViewsFromCurrentProject = config["Umbraco:CMS:Content:ResolveViewsFromCurrentProject"],
                        UnpublishedSupport = config["Umbraco:CMS:Unpublished:SupportUnpublished"]
                    };

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(umbracoConfig);
                });
            });

            // Endpoint для симуляции поиска шаблона Umbraco
            app.Map("/debug/template-simulation/{templateName}", diagnosticApp =>
            {
                diagnosticApp.Run(async context =>
                {
                    var templateName = context.Request.RouteValues["templateName"] as string ?? "BlockGrid";
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    var basePath = env.ContentRootPath;
                    
                    logger.LogInformation($"Симуляция поиска шаблона: {templateName}");

                    // Пробуем различные пути, которые может использовать Umbraco
                    var searchPaths = new[]
                    {
                        $"~/Views/{templateName}.cshtml",
                        $"~/Views/{templateName}/Index.cshtml",
                        $"~/Views/{templateName}/{templateName}.cshtml",
                        $"~/Views/{templateName}.cshtml",
                        $"/Views/{templateName}.cshtml",
                        $"/Views/{templateName}/Index.cshtml"
                    };

                    var physicalPaths = searchPaths.Select(sp =>
                    {
                        var relativePath = sp.Replace("~/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
                        var fullPath = Path.Combine(basePath, relativePath);
                        return new
                        {
                            VirtualPath = sp,
                            PhysicalPath = fullPath,
                            Exists = File.Exists(fullPath)
                        };
                    }).ToList();

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        TemplateName = templateName,
                        SearchPaths = physicalPaths,
                        CurrentDirectory = basePath
                    });
                });
            });

            // Middleware для логирования всех запросов к шаблонам
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/") || 
                    context.Request.Path.Value?.Contains("umbraco/rendermvc") == true)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogInformation($"Запрос: {context.Request.Method} {context.Request.Path}");
                    
                    if (context.Request.Path == "/")
                    {
                        logger.LogInformation("Главная страница - проверяем шаблоны...");
                        
                        var viewsPath = Path.Combine(env.ContentRootPath, "Views");
                        logger.LogInformation($"Путь к Views: {viewsPath}");
                        
                        var blockGridPath = Path.Combine(viewsPath, "BlockGrid.cshtml");
                        var testTemplatePath = Path.Combine(viewsPath, "TestTemplate.cshtml");
                        
                        logger.LogInformation($"BlockGrid exists: {File.Exists(blockGridPath)} at {blockGridPath}");
                        logger.LogInformation($"TestTemplate exists: {File.Exists(testTemplatePath)} at {testTemplatePath}");
                        
                        // Проверяем права доступа
                        if (File.Exists(blockGridPath))
                        {
                            try
                            {
                                using var fs = File.OpenRead(blockGridPath);
                                logger.LogInformation("BlockGrid файл доступен для чтения");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Нет доступа к файлу BlockGrid");
                            }
                        }
                    }
                }
                
                await next();
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
        }
    }
}