using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;

namespace MVCApp.Infrastructure.ViewComponents
{
    public class TimerViewComponent : ViewComponent
    {
        private readonly ILogger<TimerViewComponent> _logger;

        public TimerViewComponent(ILogger<TimerViewComponent> logger)
        {
            _logger = logger;
        }

        public IViewComponentResult Invoke(bool includeSeconds, bool format24 = true)
        {
            _logger.LogDebug($"Timer component invoked with params: {nameof(includeSeconds)}={includeSeconds}, {nameof(format24)}={format24}");

            string format = $"{(format24 ? "HH" : "hh")}:mm{(includeSeconds ? ":ss" : "")}{(format24 ? "" : " tt")}";
            return Content($"Time is: {DateTime.Now.ToString(format)}");
        }
    }

    public class TimerHtmlViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return new HtmlContentViewComponentResult(
                new HtmlString($"Time is: <b>{DateTime.Now:t}</b>")
                );
        }
    }
}
