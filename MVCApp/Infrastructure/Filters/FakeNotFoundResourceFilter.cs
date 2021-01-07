using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVCApp.Infrastructure.Filters
{
    public class FakeNotFoundResourceFilter : Attribute, IAsyncResourceFilter
    {
        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            context.Result = new NotFoundResult();
            return Task.CompletedTask;
        }
    }
}
