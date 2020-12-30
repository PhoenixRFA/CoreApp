using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MVCApp.Infrastructure.ValueProviders
{
    public class CookieValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IRequestCookieCollection cookies = context.ActionContext.HttpContext.Request.Cookies;
            if (cookies != null && cookies.Count > 0)
            {
                var provider = new CookieValueProvider(BindingSource.ModelBinding, cookies);

                context.ValueProviders.Add(provider);
            }

            return Task.CompletedTask;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromCookieAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        

        public BindingSource BindingSource
        {
            get
            {
                var bs = new BindingSource("Cookie", "Cookie", false, true);
                
                return bs;
            }
        }

        public string Name { get; set; }
    }

    public class MyBindingSource : BindingSource
    {
        public MyBindingSource(string id, string displayName, bool isGreedy, bool isFromRequest) : base(id, displayName, isGreedy, isFromRequest) { }

        public override bool CanAcceptDataFrom(BindingSource bindingSource)
        {
            if (bindingSource == null)
            {
                throw new ArgumentNullException(nameof(bindingSource));
            }
 
            if (this == bindingSource)
            {
                return true;
            }
 
            //There the magic happens!
            if (bindingSource == ModelBinding)
            {
                return true;
            }
 
            return false;
        }
    }
}
