// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.MarginApi.Models
{
    using LkeServices.Generated;
    using LkeServices.Generated.MarginApi;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class MtBackendResponseTradingConditionModel
    {
        /// <summary>
        /// Initializes a new instance of the
        /// MtBackendResponseTradingConditionModel class.
        /// </summary>
        public MtBackendResponseTradingConditionModel()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// MtBackendResponseTradingConditionModel class.
        /// </summary>
        public MtBackendResponseTradingConditionModel(TradingConditionModel result = default(TradingConditionModel), string message = default(string))
        {
            Result = result;
            Message = message;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "result")]
        public TradingConditionModel Result { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Result != null)
            {
                Result.Validate();
            }
        }
    }
}
