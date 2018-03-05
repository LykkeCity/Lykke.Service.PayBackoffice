using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Fee
{
    public class Fee
    {
        public double BankCardsFeeSizePercentage { get; set; }

        [JsonProperty("CashOut")]
        public List<CashOutFee> CashOutFees { get; set; }
    }

    public class CashOutFee
    {
        public string AssetId { get; set; }

        public double Size { get; set; }

        [JsonProperty("Type")]
        public string FeeType { get; set; }
    }
}
