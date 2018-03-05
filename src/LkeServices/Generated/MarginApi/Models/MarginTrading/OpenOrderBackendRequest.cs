// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.MarginApi.Models
{
    using LkeServices.Generated;
    using LkeServices.Generated.MarginApi;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class OpenOrderBackendRequest
    {
        /// <summary>
        /// Initializes a new instance of the OpenOrderBackendRequest class.
        /// </summary>
        public OpenOrderBackendRequest()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the OpenOrderBackendRequest class.
        /// </summary>
        public OpenOrderBackendRequest(string clientId = default(string), NewOrderBackendContract order = default(NewOrderBackendContract))
        {
            ClientId = clientId;
            Order = order;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "order")]
        public NewOrderBackendContract Order { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Order != null)
            {
                Order.Validate();
            }
        }
    }
}
