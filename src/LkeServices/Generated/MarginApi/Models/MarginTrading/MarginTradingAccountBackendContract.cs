// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.MarginApi.Models
{
    using LkeServices.Generated;
    using LkeServices.Generated.MarginApi;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class MarginTradingAccountBackendContract
    {
        /// <summary>
        /// Initializes a new instance of the
        /// MarginTradingAccountBackendContract class.
        /// </summary>
        public MarginTradingAccountBackendContract()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// MarginTradingAccountBackendContract class.
        /// </summary>
        public MarginTradingAccountBackendContract(double balance, double withdrawTransferLimit, double marginCall, double stopOut, double totalCapital, double freeMargin, double marginAvailable, double usedMargin, double marginInit, double pnL, int openPositionsCount, double marginUsageLevel, bool isLive, string id = default(string), string clientId = default(string), string tradingConditionId = default(string), string baseAssetId = default(string))
        {
            Id = id;
            ClientId = clientId;
            TradingConditionId = tradingConditionId;
            BaseAssetId = baseAssetId;
            Balance = balance;
            WithdrawTransferLimit = withdrawTransferLimit;
            MarginCall = marginCall;
            StopOut = stopOut;
            TotalCapital = totalCapital;
            FreeMargin = freeMargin;
            MarginAvailable = marginAvailable;
            UsedMargin = usedMargin;
            MarginInit = marginInit;
            PnL = pnL;
            OpenPositionsCount = openPositionsCount;
            MarginUsageLevel = marginUsageLevel;
            IsLive = isLive;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }

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
        [JsonProperty(PropertyName = "balance")]
        public double Balance { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "withdrawTransferLimit")]
        public double WithdrawTransferLimit { get; set; }

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
        [JsonProperty(PropertyName = "totalCapital")]
        public double TotalCapital { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "freeMargin")]
        public double FreeMargin { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "marginAvailable")]
        public double MarginAvailable { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "usedMargin")]
        public double UsedMargin { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "marginInit")]
        public double MarginInit { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "pnL")]
        public double PnL { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openPositionsCount")]
        public int OpenPositionsCount { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "marginUsageLevel")]
        public double MarginUsageLevel { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "isLive")]
        public bool IsLive { get; set; }

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
