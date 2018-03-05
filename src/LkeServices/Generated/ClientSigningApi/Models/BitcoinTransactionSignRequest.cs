// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.ClientSigningApi.Models
{
    using System.Linq;

    public partial class BitcoinTransactionSignRequest
    {
        /// <summary>
        /// Initializes a new instance of the BitcoinTransactionSignRequest
        /// class.
        /// </summary>
        public BitcoinTransactionSignRequest() { }

        /// <summary>
        /// Initializes a new instance of the BitcoinTransactionSignRequest
        /// class.
        /// </summary>
        /// <param name="hashType">Possible values include: 'Undefined',
        /// 'All', 'None', 'Single', 'AnyoneCanPay'</param>
        public BitcoinTransactionSignRequest(string transaction = default(string), string hashType = default(string), System.Collections.Generic.IList<string> additionalSecrets = default(System.Collections.Generic.IList<string>))
        {
            Transaction = transaction;
            HashType = hashType;
            AdditionalSecrets = additionalSecrets;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "transaction")]
        public string Transaction { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Undefined', 'All', 'None',
        /// 'Single', 'AnyoneCanPay'
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "hashType")]
        public string HashType { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "additionalSecrets")]
        public System.Collections.Generic.IList<string> AdditionalSecrets { get; set; }

    }
}
