using System.Threading.Tasks;
using Core.CashOperations.PaymentSystems;
using Lykke.Messages.Email;
using Lykke.Service.Kyc.Abstractions.Domain.Documents;

namespace Core.Messages
{
    public interface ISrvEmailsFacade
    {
        Task SendWelcomeFxEmail(string partnerId, string email, string clientId);

        Task<string> SendConfirmEmail(string partnerId, string email, bool generateRealCode, bool createPriorityCode = false);

        Task SendUserRegisteredKycBroadcast(string partnerId, string clientId);

        Task SendRejectedEmail(string partnerId, string email);

        Task SendSwiftEmail(string partnerId, string email, string clientId, double balanceChange, string assetId);

        Task SendBackupPrivateWalletEmail(string partnerId, string email, string walletName, string walletAddress,
            string question, string encodedKey);

        Task SendRemindPasswordEmail(string partnerId, string email, string hint);

        Task SendPlainTextBroadcast(string partnerId, BroadcastGroup @group, string subject, string text);

        Task SendSolarCashOutCompletedEmail(string partnerId, string email, string addressTo, double amount);

        Task SendSwiftConfirmedEmail(string partnerId, BroadcastGroup @group, string userEmail, double amount, string assetId, string accNum,
            string accName, string bic, string bankName, string accHolderAddress, string hash);

        Task SendDocumentsDeclined(string partnerId, string email, string fullName, KycDocument[] documents);

        Task SendRequestForDocument(string partnerId, string email, string fullName, string text, string comment);

        Task SendSwiftCashoutProcessedEmail(string partnerId, string email, string fullname, double amount, string assetId, Swift data);

        Task SendSwiftCashoutDeclinedEmail(string partnerId, string email, string fullname, string text, string comment);
    }
}
