// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.ClientSigningApi.Models
{
    using System.Linq;

    public partial class TransactionIdResponse
    {
        /// <summary>
        /// Initializes a new instance of the TransactionIdResponse class.
        /// </summary>
        public TransactionIdResponse() { }

        /// <summary>
        /// Initializes a new instance of the TransactionIdResponse class.
        /// </summary>
        public TransactionIdResponse(System.Guid? transactionId = default(System.Guid?))
        {
            TransactionId = transactionId;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "transactionId")]
        public System.Guid? TransactionId { get; set; }

    }
}
