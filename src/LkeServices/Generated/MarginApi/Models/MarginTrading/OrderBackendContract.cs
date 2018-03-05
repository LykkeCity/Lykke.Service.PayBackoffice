// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.MarginApi.Models
{
    using LkeServices.Generated;
    using LkeServices.Generated.MarginApi;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class OrderBackendContract
    {
        /// <summary>
        /// Initializes a new instance of the OrderBackendContract class.
        /// </summary>
        public OrderBackendContract()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the OrderBackendContract class.
        /// </summary>
        /// <param name="type">Possible values include: 'Buy', 'Sell'</param>
        /// <param name="status">Possible values include:
        /// 'WaitingForExecution', 'Active', 'Closed', 'Rejected',
        /// 'Closing'</param>
        /// <param name="closeReason">Possible values include: 'None', 'Close',
        /// 'StopLoss', 'TakeProfit', 'StopOut', 'Canceled',
        /// 'CanceledBySystem', 'ClosedByBroker'</param>
        /// <param name="rejectReason">Possible values include: 'None',
        /// 'NoLiquidity', 'NotEnoughBalance', 'LeadToStopOut',
        /// 'AccountInvalidState', 'InvalidExpectedOpenPrice', 'InvalidVolume',
        /// 'InvalidTakeProfit', 'InvalidStoploss', 'InvalidInstrument',
        /// 'InvalidAccount', 'TradingConditionError', 'TechnicalError'</param>
        public OrderBackendContract(OrderDirectionContract type, OrderStatusContract status, OrderCloseReasonContract closeReason, OrderRejectReasonContract rejectReason, double openPrice, double closePrice, double volume, double matchedVolume, double matchedCloseVolume, double commissionLot, double openCommission, double closeCommission, double swapCommission, string id = default(string), string accountId = default(string), string instrument = default(string), string rejectReasonText = default(string), double? expectedOpenPrice = default(double?), System.DateTime? openDate = default(System.DateTime?), System.DateTime? closeDate = default(System.DateTime?), double? takeProfit = default(double?), double? stopLoss = default(double?), double? fpl = default(double?))
        {
            Id = id;
            AccountId = accountId;
            Instrument = instrument;
            Type = type;
            Status = status;
            CloseReason = closeReason;
            RejectReason = rejectReason;
            RejectReasonText = rejectReasonText;
            ExpectedOpenPrice = expectedOpenPrice;
            OpenPrice = openPrice;
            ClosePrice = closePrice;
            OpenDate = openDate;
            CloseDate = closeDate;
            Volume = volume;
            MatchedVolume = matchedVolume;
            MatchedCloseVolume = matchedCloseVolume;
            TakeProfit = takeProfit;
            StopLoss = stopLoss;
            Fpl = fpl;
            CommissionLot = commissionLot;
            OpenCommission = openCommission;
            CloseCommission = closeCommission;
            SwapCommission = swapCommission;
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
        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "instrument")]
        public string Instrument { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Buy', 'Sell'
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public OrderDirectionContract Type { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'WaitingForExecution',
        /// 'Active', 'Closed', 'Rejected', 'Closing'
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public OrderStatusContract Status { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'None', 'Close', 'StopLoss',
        /// 'TakeProfit', 'StopOut', 'Canceled', 'CanceledBySystem',
        /// 'ClosedByBroker'
        /// </summary>
        [JsonProperty(PropertyName = "closeReason")]
        public OrderCloseReasonContract CloseReason { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'None', 'NoLiquidity',
        /// 'NotEnoughBalance', 'LeadToStopOut', 'AccountInvalidState',
        /// 'InvalidExpectedOpenPrice', 'InvalidVolume', 'InvalidTakeProfit',
        /// 'InvalidStoploss', 'InvalidInstrument', 'InvalidAccount',
        /// 'TradingConditionError', 'TechnicalError'
        /// </summary>
        [JsonProperty(PropertyName = "rejectReason")]
        public OrderRejectReasonContract RejectReason { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "rejectReasonText")]
        public string RejectReasonText { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "expectedOpenPrice")]
        public double? ExpectedOpenPrice { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openPrice")]
        public double OpenPrice { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "closePrice")]
        public double ClosePrice { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openDate")]
        public System.DateTime? OpenDate { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "closeDate")]
        public System.DateTime? CloseDate { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "volume")]
        public double Volume { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "matchedVolume")]
        public double MatchedVolume { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "matchedCloseVolume")]
        public double MatchedCloseVolume { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "takeProfit")]
        public double? TakeProfit { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "stopLoss")]
        public double? StopLoss { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "fpl")]
        public double? Fpl { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "commissionLot")]
        public double CommissionLot { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openCommission")]
        public double OpenCommission { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "closeCommission")]
        public double CloseCommission { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "swapCommission")]
        public double SwapCommission { get; set; }

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
