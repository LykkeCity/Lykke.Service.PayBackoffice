using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Extensions
{
    public static class ModelStateExtensions
    {
        public static string GetAllErrors(this ModelStateDictionary modelState)
        {
            if (modelState.IsValid)
                return string.Empty;

            var modelErrors = new List<string>();
            foreach (var modelStateVal in modelState.Values)
            {
                foreach (var modelError in modelStateVal.Errors)
                {
                    modelErrors.Add(modelError.ErrorMessage);
                }
            }

            return string.Join("\r\n", modelErrors);
        }
    }
}
