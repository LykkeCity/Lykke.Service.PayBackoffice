using System;
using System.Globalization;    
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BackOffice.ModelBinders
{
    public class DecimalInvariantBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));
            
            string modelName = bindingContext.ModelName;

            if (string.IsNullOrEmpty(modelName))
                return Task.CompletedTask;
            
            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(modelName,
                valueProviderResult);

            string value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
                return Task.CompletedTask;

            if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var model))
            {
                bindingContext.Result = ModelBindingResult.Success(model);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid value");
            }
            
            return Task.CompletedTask;
        }
    }
}
