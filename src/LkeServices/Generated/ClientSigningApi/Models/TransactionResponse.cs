// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.ClientSigningApi.Models
{
    using System.Linq;

    public partial class TransactionResponse
    {
        /// <summary>
        /// Initializes a new instance of the TransactionResponse class.
        /// </summary>
        public TransactionResponse() { }

        /// <summary>
        /// Initializes a new instance of the TransactionResponse class.
        /// </summary>
        public TransactionResponse(string transaction = default(string), System.Guid? transactionId = default(System.Guid?))
        {
            Transaction = transaction;
            TransactionId = transactionId;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "transaction")]
        public string Transaction { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "transactionId")]
        public System.Guid? TransactionId { get; set; }

    }
}
