// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.MarginApi.Models
{
    using LkeServices.Generated;
    using LkeServices.Generated.MarginApi;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class AccountGroupModel
    {
        /// <summary>
        /// Initializes a new instance of the AccountGroupModel class.
        /// </summary>
        public AccountGroupModel()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AccountGroupModel class.
        /// </summary>
        public AccountGroupModel(double marginCall, double stopOut, double depositTransferLimit, double profitWithdrawalLimit, string tradingConditionId = default(string), string baseAssetId = default(string))
        {
            TradingConditionId = tradingConditionId;
            BaseAssetId = baseAssetId;
            MarginCall = marginCall;
            StopOut = stopOut;
            DepositTransferLimit = depositTransferLimit;
            ProfitWithdrawalLimit = profitWithdrawalLimit;
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

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "marginCall")]
        public double MarginCall { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "stopOut")]
        public double StopOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "depositTransferLimit")]
        public double DepositTransferLimit { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "profitWithdrawalLimit")]
        public double ProfitWithdrawalLimit { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            //Nothing to validate
        }
    }
}
