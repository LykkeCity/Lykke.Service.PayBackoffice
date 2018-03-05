// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.MarginApi.Models
{
    using LkeServices.Generated;
    using LkeServices.Generated.MarginApi;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class InitAccountGroupRequest
    {
        /// <summary>
        /// Initializes a new instance of the InitAccountGroupRequest class.
        /// </summary>
        public InitAccountGroupRequest()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the InitAccountGroupRequest class.
        /// </summary>
        public InitAccountGroupRequest(string tradingConditionId = default(string), string baseAssetId = default(string))
        {
            TradingConditionId = tradingConditionId;
            BaseAssetId = baseAssetId;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "tradingConditionId")]
        public string TradingConditionId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "baseAssetId")]
        public string BaseAssetId { get; set; }

    }
}
