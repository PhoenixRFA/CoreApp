using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Infrastructure.ViewComponents
{
    public class AsyncTestViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int delay = 1000)
        {
            await Task.Delay(delay);

            return Content($"Rendered after {delay}ms delay");
        }
    }
}
