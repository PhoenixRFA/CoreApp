using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace MVCApp.Infrastructure.ModelBinders
{
    public class CustomDateTimeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(DateTime))
            {
                return new CustomDateTimeModelBinder(); //new BinderTypeModelBinder(typeof(CustomDateTimeModelBinder));
            }

            return null;
        }
    }
}
