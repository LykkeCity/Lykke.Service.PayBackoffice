using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.PaymentSystems
{
    public interface IPaymentTransaction
    {
        string Id { get; }

        string ClientId { get; }

        double Amount { get; }
        
        string AssetId { get; }

        string WalletId { get; }

        /// <summary>
        /// Amount of asset we deposit account
        /// </summary>
        double? DepositedAmount { get; }

        string DepositedAssetId { get; }


        double? Rate { get; }


        string AggregatorTransactionId { get; }
        
        DateTime Created { get; }

        PaymentStatus Status { get; }

        CashInPaymentSystem PaymentSystem { get; }

        string Info { get; }

        double FeeAmount { get; }
        string MeTransactionId { get; set; }
    }


    public enum PaymentStatus
    {
        Created,
        NotifyProcessed,
        NotifyDeclined,
        Processing
    }


    public class PaymentTransaction : IPaymentTransaction
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string WalletId { get; set; }
        public double? DepositedAmount { get; set; }
        public string DepositedAssetId { get; set; }
        public double? Rate { get; set; }
        public string AggregatorTransactionId { get; set; }
        public DateTime Created { get; set; }
        public PaymentStatus Status { get; set; }
        public CashInPaymentSystem PaymentSystem { get; set; }
        public string Info { get; set; }

        public string OtherData { get; set; }

        public double FeeAmount { get; set; }
        public string MeTransactionId { get; set; }

        public static PaymentTransaction Create(string id,
            CashInPaymentSystem paymentSystem,
            string clientId,
            double amount,
            double feeAmount,
            string assetId,
            string walletId,
            string assetToDeposit = null,
            string info = "",
            PaymentStatus status = PaymentStatus.Created)
        {
           
            return new PaymentTransaction
            {
                Id = id,
                PaymentSystem = paymentSystem,
                ClientId = clientId,
                Amount = amount,
                AssetId = assetId,
                WalletId = walletId,
                Created = DateTime.UtcNow,
                Status = status,
                Info = info,
                DepositedAssetId = assetToDeposit ?? assetId,
                FeeAmount = feeAmount
            };
        }
    }

    public interface IPaymentTransactionsRepository
    {
        Task CreateAsync(IPaymentTransaction paymentTransaction);

        Task<IEnumerable<IPaymentTransaction>> GetAsync(DateTime from, DateTime to, Func<IPaymentTransaction, bool> filter);

        Task<IEnumerable<IPaymentTransaction>> GetByClientIdAsync(string clientId);

        Task<IPaymentTransaction> GetByTransactionIdAsync(string id);

        Task<IPaymentTransaction> TryCreateAsync(IPaymentTransaction paymentTransaction);

        /// <summary>
        /// Change transaction to process state and check if it's already processed or started being processed
        /// </summary>
        /// <param name="id">it of transaction</param>
        /// <param name="paymentAggregatorTransactionId">id of payment aggregator to update if transaction can be processed</param>
        /// <returns>null - transaction is not exists or can not be processed</returns>
        Task<IPaymentTransaction> StartProcessingTransactionAsync(string id, string paymentAggregatorTransactionId = null);

        Task<IPaymentTransaction> SetStatus(string id, PaymentStatus status);

        Task<IPaymentTransaction> SetAsOkAsync(string id, double depositedAmount, double? rate);

        Task<IPaymentTransaction> GetLastByDate(string clientId);

        Task<IPaymentTransaction> SetAggregatorTransactionId(string id, string aggregatorTransactionId);


        Task<IEnumerable<IPaymentTransaction>> ScanAndFindAsync(Func<IPaymentTransaction, bool> callback);
    }


    public static class PaymentTransactionExt
    {

        public static object GetInfo(this IPaymentTransaction src, Type expectedType = null, bool throwExeption = false)
        {

            if (!PaymentSystemsAndOtherInfo.PsAndOtherInfoLinks.ContainsKey(src.PaymentSystem))
            {
                if (throwExeption)
                    throw new Exception("Unsupported payment system for reading other info: transactionId:" + src.Id);

                return null;
            }


            var type = PaymentSystemsAndOtherInfo.PsAndOtherInfoLinks[src.PaymentSystem];

            if (expectedType != null)
            {
                if (type != expectedType)
                    throw new Exception("Payment system and Other info does not match for transactionId:" + src.Id);
            }


            return Newtonsoft.Json.JsonConvert.DeserializeObject(src.Info, type);

        }

        public static T GetInfo<T>(this IPaymentTransaction src)
        {
            return (T) GetInfo(src, typeof(T), true);
        }


        public static bool AreMoneyOnOurAccount(this IPaymentTransaction src)
        {
            return src.Status == PaymentStatus.NotifyProcessed;
        }
    }


}
