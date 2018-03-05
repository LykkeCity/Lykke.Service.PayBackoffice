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

    public partial class CloseAccountPositionsRequest
    {
        /// <summary>
        /// Initializes a new instance of the CloseAccountPositionsRequest
        /// class.
        /// </summary>
        public CloseAccountPositionsRequest()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the CloseAccountPositionsRequest
        /// class.
        /// </summary>
        public CloseAccountPositionsRequest(bool ignoreMarginLevel, IList<string> accountIds = default(IList<string>))
        {
            AccountIds = accountIds;
            IgnoreMarginLevel = ignoreMarginLevel;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "accountIds")]
        public IList<string> AccountIds { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "ignoreMarginLevel")]
        public bool IgnoreMarginLevel { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
        }
    }
}
