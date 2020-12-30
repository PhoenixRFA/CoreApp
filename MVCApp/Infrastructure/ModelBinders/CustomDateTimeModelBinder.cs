using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MVCApp.Infrastructure.ModelBinders
{
    public class CustomDateTimeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            string modelName = bindingContext.ModelName;
            ValueProviderResult datePartValue = bindingContext.ValueProvider.GetValue("Date");
            ValueProviderResult timePartValue = bindingContext.ValueProvider.GetValue("Time");

            if (datePartValue == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            string date = datePartValue.FirstValue;
            DateTime.TryParse(date, out DateTime parsedDate);


            DateTime res;
            
            if (timePartValue == ValueProviderResult.None)
            {
                res = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day);
            }
            else
            {
                string time = timePartValue.FirstValue;
                DateTime.TryParse(time, out DateTime parsedTime);
                res = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, parsedTime.Hour, parsedTime.Minute, parsedTime.Second);
            }

            bindingContext.Result = ModelBindingResult.Success(res);
            return Task.CompletedTask;
        }
    }
}
