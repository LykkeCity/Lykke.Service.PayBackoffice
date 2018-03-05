// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.MarginApi.Models
{
    using LkeServices.Generated;
    using LkeServices.Generated.MarginApi;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class OrderbooksBackendRequest
    {
        /// <summary>
        /// Initializes a new instance of the OrderbooksBackendRequest class.
        /// </summary>
        public OrderbooksBackendRequest()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the OrderbooksBackendRequest class.
        /// </summary>
        public OrderbooksBackendRequest(string instrument = default(string))
        {
            Instrument = instrument;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "instrument")]
        public string Instrument { get; set; }

    }
}
