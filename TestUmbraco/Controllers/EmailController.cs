using TestUmbraco.Application.Contracts;
using TestUmbraco.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using reCAPTCHA.AspNetCore.Attributes;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace TestUmbraco.Controllers
{
    public class EmailController : SurfaceController
    {
        private readonly IEmailService _emailService;
        private readonly INotyfService _notifyService;

        public EmailController(IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches, IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IEmailService emailService, 
            INotyfService notifyService)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _emailService = emailService;
            _notifyService = notifyService;
        }

        [HttpPost]
        [ValidateRecaptcha(0.5)]
        public async Task<IActionResult> SendEmailRequest(EmailRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _notifyService.Error("При отправлении заявки произошла ошибка");
                return CurrentUmbracoPage();
            }
            await _emailService.SendEmailRequestAsync(request);
            _notifyService.Success("Ваш запрос успешно отправлен!");
            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateRecaptcha(0.5)]
        public async Task<IActionResult> SendCallRequest(CallRequestDto callRequest)
        {
            if (!ModelState.IsValid)
            {
                _notifyService.Error("При отправлении заявки произошла ошибка");
                return CurrentUmbracoPage();
            }
            await _emailService.SendCallRequestAsync(callRequest);
            _notifyService.Success("Ваш запрос успешно отправлен!");
            return RedirectToCurrentUmbracoPage();
        }

        public IActionResult Error()
        {
            return CurrentUmbracoPage();
        }
    }
}
