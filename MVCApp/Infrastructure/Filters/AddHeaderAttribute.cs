using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVCApp.Infrastructure.Filters
{
    public class AddHeaderAttribute : Attribute, IResourceFilter
    {
        private readonly string _name;
        private readonly string _value;

        public AddHeaderAttribute(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            _name = name;
            _value = value;
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add(_name, _value);
        }

        public void OnResourceExecuted(ResourceExecutedContext context) { }
    }
}
