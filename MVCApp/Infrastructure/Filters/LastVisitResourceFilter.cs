using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVCApp.Infrastructure.Filters
{
    public class LastVisitResourceFilter : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("LastVisit", DateTime.Now.ToString("O"));
        }

        public void OnResourceExecuted(ResourceExecutedContext context) { }
    }
}
