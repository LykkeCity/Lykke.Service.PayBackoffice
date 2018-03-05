using System.Threading.Tasks;
using Core.Wallets;

namespace Core.PaymentSystems
{
    public interface IPaymentSystemFacade
    {
        bool IsAssetIdSupported(
            string assetId,
            string isoCountryCode,
            string clientPaymentSystem,
            OwnerType owner);
        
        Task<PaymentUrlData> GetUrlDataAsync(
            string clientPaymentSystem,
            string orderId,
            string clientId,
            double amount,
            string assetId,
            string walletId,
            string isoCountryCode,
            string otherInfoJson);
        
        Task<string> GetSourceClientIdAsync(CashInPaymentSystem paymentSystem, OwnerType owner);
    }
}
