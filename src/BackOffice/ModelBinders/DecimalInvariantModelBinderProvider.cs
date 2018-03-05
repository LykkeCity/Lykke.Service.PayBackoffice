using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace BackOffice.ModelBinders
{
    public class DecimalInvariantModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Metadata.ModelType == typeof(decimal)
                ? new BinderTypeModelBinder(typeof(DecimalInvariantBinder)) 
                : null;
        }
    }
}
