// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.MarginApi.Models
{
    using LkeServices.Generated;
    using LkeServices.Generated.MarginApi;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class InitPricesBackendRequest
    {
        /// <summary>
        /// Initializes a new instance of the InitPricesBackendRequest class.
        /// </summary>
        public InitPricesBackendRequest()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the InitPricesBackendRequest class.
        /// </summary>
        public InitPricesBackendRequest(IList<string> assetIds = default(IList<string>), string clientId = default(string))
        {
            AssetIds = assetIds;
            ClientId = clientId;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "assetIds")]
        public IList<string> AssetIds { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }

    }
}
