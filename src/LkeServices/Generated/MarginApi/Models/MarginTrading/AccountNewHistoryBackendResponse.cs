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

    public partial class AccountNewHistoryBackendResponse
    {
        /// <summary>
        /// Initializes a new instance of the AccountNewHistoryBackendResponse
        /// class.
        /// </summary>
        public AccountNewHistoryBackendResponse()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AccountNewHistoryBackendResponse
        /// class.
        /// </summary>
        public AccountNewHistoryBackendResponse(IList<AccountHistoryItemBackend> historyItems = default(IList<AccountHistoryItemBackend>))
        {
            HistoryItems = historyItems;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "historyItems")]
        public IList<AccountHistoryItemBackend> HistoryItems { get; set; }

    }
}
