using System;
using System.Threading.Tasks;
using Core.PaymentSystems;
using Core.Settings;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.FeeCalculator.Client;
using Core.Wallets;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ExchangeOperations.Client;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace LkeServices.PaymentSystems
{
    public interface IPaymentOkNotification
    {
        Task NotifyAsync(IPaymentTransaction paymentTransaction);
    }

    public class SrvPaymentProcessor
    {
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly IPaymentSystemFacade _paymentSystemFacade;
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IPaymentTransactionEventsLog _paymentTransactionEventsLog;
        private readonly IPaymentOkNotification[] _paymentOkNotifications;
        private readonly FeeSettings _feeSettings;
        private readonly IFeeCalculatorClient _feeCalculatorClient;
        private readonly IClientAccountClient _clientAccountClient;

        public SrvPaymentProcessor(
            IExchangeOperationsServiceClient exchangeOperationsService,
            IPaymentSystemFacade paymentSystemFacade,
            IPaymentTransactionsRepository paymentTransactionsRepository,
            IPaymentTransactionEventsLog paymentTransactionEventsLog,
            IPaymentOkNotification[] paymentOkNotifications,
            FeeSettings feeSettings,
            IFeeCalculatorClient feeCalculatorClient,
            IClientAccountClient clientAccountClient)
        {
            _exchangeOperationsService = exchangeOperationsService;
            _paymentSystemFacade = paymentSystemFacade;
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _paymentTransactionEventsLog = paymentTransactionEventsLog;
            _paymentOkNotifications = paymentOkNotifications;
            _feeSettings = feeSettings;
            _feeCalculatorClient = feeCalculatorClient ?? throw new ArgumentNullException(nameof(feeCalculatorClient));
            _clientAccountClient = clientAccountClient;
        }

        public async Task<bool> NotifyAsOkAsync(string transactionId, string paymentSystemTransactionId, string who)
        {
            var pt = await _paymentTransactionsRepository.StartProcessingTransactionAsync(transactionId, paymentSystemTransactionId);

            if (pt == null)
            {
                await _paymentTransactionEventsLog.WriteAsync(
                    PaymentTransactionLogEvent.Create(transactionId, "N/A", "Transaction not found or already processed", who));
                return false;
            }
            
            var bankCardFees = await _feeCalculatorClient.GetBankCardFees();

            var owner = OwnerType.Spot;

            if (!string.IsNullOrEmpty(pt.WalletId))
            {
                var wallet = await _clientAccountClient.GetWalletAsync(pt.WalletId);

                if (wallet == null)
                    throw new Exception($"Wallet with ID {pt.WalletId} was not found");

                if (!Enum.TryParse(wallet.Owner, out owner))
                {
                    throw new Exception($"Owner {wallet.Owner} is not supported");
                }
            }

            string sourceClientId = await _paymentSystemFacade.GetSourceClientIdAsync(pt.PaymentSystem, owner);
            var result = await _exchangeOperationsService.TransferWithNotificationAsync(
                pt.MeTransactionId,
                pt.ClientId,
                sourceClientId,
                pt.Amount,
                pt.AssetId,
                _feeSettings.TargetClientId.BankCard,
                bankCardFees.Percentage);

            if (!result.IsOk())
            {
                await _paymentTransactionEventsLog.WriteAsync(
                    PaymentTransactionLogEvent.Create(transactionId, "N/A", $"{result.Code}:{result.Message}", who));
                return false;
            }

            var resultTransaction = await _paymentTransactionsRepository.SetAsOkAsync(transactionId, pt.Amount, null);

            await _paymentTransactionEventsLog.WriteAsync(
                PaymentTransactionLogEvent.Create(transactionId, "", "Transaction processed as Ok", who));

            foreach (var notification in _paymentOkNotifications)
                await notification.NotifyAsync(resultTransaction);

            return true;
        }

        public async Task NotifyFailAsync(string transactionId, string paymentSystemTransactionId, string who)
        {
            await _paymentTransactionsRepository.SetStatus(transactionId, PaymentStatus.NotifyDeclined);

            await _paymentTransactionEventsLog.WriteAsync(
                PaymentTransactionLogEvent.Create(
                    transactionId, paymentSystemTransactionId, "Declined by Payment status from payment system", who));
        }
    }
}
