// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace LkeServices.Generated.MarginApi.Models
{
    using LkeServices.Generated;
    using LkeServices.Generated.MarginApi;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines values for OrderDirectionContract.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderDirectionContract
    {
        [EnumMember(Value = "Buy")]
        Buy,
        [EnumMember(Value = "Sell")]
        Sell
    }
    internal static class OrderDirectionContractEnumExtension
    {
        internal static string ToSerializedValue(this OrderDirectionContract? value)  =>
            value == null ? null : ((OrderDirectionContract)value).ToSerializedValue();

        internal static string ToSerializedValue(this OrderDirectionContract value)
        {
            switch( value )
            {
                case OrderDirectionContract.Buy:
                    return "Buy";
                case OrderDirectionContract.Sell:
                    return "Sell";
            }
            return null;
        }

        internal static OrderDirectionContract? ParseOrderDirectionContract(this string value)
        {
            switch( value )
            {
                case "Buy":
                    return OrderDirectionContract.Buy;
                case "Sell":
                    return OrderDirectionContract.Sell;
            }
            return null;
        }
    }
}
