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

    public partial class OrderBookBackendContract
    {
        /// <summary>
        /// Initializes a new instance of the OrderBookBackendContract class.
        /// </summary>
        public OrderBookBackendContract()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the OrderBookBackendContract class.
        /// </summary>
        public OrderBookBackendContract(IDictionary<string, IList<LimitOrderBackendContract>> buy = default(IDictionary<string, IList<LimitOrderBackendContract>>), IDictionary<string, IList<LimitOrderBackendContract>> sell = default(IDictionary<string, IList<LimitOrderBackendContract>>))
        {
            Buy = buy;
            Sell = sell;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "buy")]
        public IDictionary<string, IList<LimitOrderBackendContract>> Buy { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "sell")]
        public IDictionary<string, IList<LimitOrderBackendContract>> Sell { get; set; }

    }
}
