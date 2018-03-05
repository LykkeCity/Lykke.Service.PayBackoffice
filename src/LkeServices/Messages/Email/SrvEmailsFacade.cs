using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Messages;
using Core.Settings;
using Core.VerificationCode;
using Core.CashOperations.PaymentSystems;
using Lykke.Messages.Email;
using Lykke.Messages.Email.MessageData;

using Lykke.Service.Kyc.Abstractions.Domain.Documents;

namespace LkeServices.Messages.Email
{
    public class SrvEmailsFacade : ISrvEmailsFacade
    {
        private readonly IEmailVerificationCodeRepository _emailVerificationCodeRepository;
        private readonly IEmailSender _emailSender;
        private readonly SupportToolsSettings _supportToolsSettings;

        public SrvEmailsFacade(IEmailVerificationCodeRepository emailVerificationCodeRepository,
            IEmailSender emailSender, SupportToolsSettings supportToolsSettings)
        {
            _emailVerificationCodeRepository = emailVerificationCodeRepository;
            _emailSender = emailSender;
            _supportToolsSettings = supportToolsSettings;
        }

        public async Task SendWelcomeFxEmail(string partnerId, string email, string clientId)
        {
            var msgData = new KycOkData
            {
                ClientId = clientId,
                Year = DateTime.UtcNow.Year.ToString()
            };
            await _emailSender.SendEmailAsync(partnerId, email, msgData);
        }

        public async Task<string> SendConfirmEmail(string partnerId, string email, bool generateRealCode, bool createPriorityCode = false)
        {
            IEmailVerificationCode emailCode;
            if (createPriorityCode)
            {
                var expDate = DateTime.UtcNow.AddSeconds(_supportToolsSettings.PriorityCodeExpirationInterval);
                emailCode = await _emailVerificationCodeRepository.CreatePriorityAsync(email, partnerId, expDate);
            }
            else
            {
                emailCode = await _emailVerificationCodeRepository.CreateAsync(email, partnerId, generateRealCode);
            }

            var msgData = new EmailComfirmationData()
            {
                ConfirmationCode = emailCode.Code,
                Year = DateTime.UtcNow.Year.ToString()
            };

            await _emailSender.SendEmailAsync(partnerId, email, msgData);

            return emailCode.Code;
        }

        public async Task SendUserRegisteredKycBroadcast(string partnerId, string clientId)
        {
            var broadcastMsg = new UserRegisteredData() { ClientId = clientId };
            await _emailSender.SendEmailBroadcastAsync(partnerId, BroadcastGroup.Kyc, broadcastMsg);
        }

        public async Task SendRejectedEmail(string partnerId, string email)
        {
            var msgData = new RejectedData();
            await _emailSender.SendEmailAsync(partnerId, email, msgData);
        }

        public async Task SendSwiftEmail(string partnerId, string email, string clientId, double balanceChange, string assetId)
        {
            var msgData = new BankCashInData
            {
                ClientId = clientId,
                Amount = balanceChange,
                AssetId = assetId
            };

            await _emailSender.SendEmailAsync(partnerId, email, msgData);
        }

        public async Task SendBackupPrivateWalletEmail(string partnerId, string email, string walletName, string walletAddress,
            string question, string encodedKey)
        {
            var msgData = new PrivateWalletBackupData
            {
                WalletName = walletName,
                WalletAddress = walletAddress,
                EncodedKey = encodedKey,
                SecurityQuestion = question
            };
            await _emailSender.SendEmailAsync(partnerId, email, msgData);
        }

        public async Task SendRemindPasswordEmail(string partnerId, string email, string hint)
        {
            var msgData = new RemindPasswordData
            {
                PasswordHint = hint
            };

            await _emailSender.SendEmailAsync(partnerId, email, msgData);
        }

        public async Task SendPlainTextBroadcast(string partnerId, BroadcastGroup @group, string subject, string text)
        {
            var msgData = new PlainTextBroadCastData
            {
                Subject = subject,
                Text = text
            };

            await _emailSender.SendEmailBroadcastAsync(partnerId, @group, msgData);
        }

        public Task SendSolarCashOutCompletedEmail(string partnerId, string email, string addressTo, double amount)
        {
            var msgData = new SolarCashOutData
            {
                AddressTo = addressTo,
                Amount = amount
            };

            return _emailSender.SendEmailAsync(partnerId, email, msgData);
        }

        public async Task SendSwiftConfirmedEmail(string partnerId, BroadcastGroup @group, string userEmail, double amount, string assetId,
            string accNum, string accName, string bic, string bankName, string accHolderAddress, string hash)
        {
            var msgData = new SwiftConfirmedData
            {
                Amount = amount,
                AssetId = assetId,
                AccNumber = accNum,
                AccName = accName,
                BlockchainHash = hash,
                Bic = bic,
                Email = userEmail,
                AccHolderAddress = accHolderAddress,
                BankName = bankName
            };

            await _emailSender.SendEmailBroadcastAsync(partnerId, @group, msgData);
        }

        public async Task SendDocumentsDeclined(string partnerId, string email, string fullName, KycDocument[] documents)
        {
            var msgData = new DeclinedDocumentsData
            {
                FullName = fullName,
                Documents = documents?.Select(x => new KycDocumentData
                {
                    ClientId = x.ClientId,
                    DocumentId = x.DocumentId,
                    Type = x.Type,
                    Mime = x.Mime,
                    KycComment = x.KycComment,
                    State = x.State,
                    FileName = x.FileName,
                    DateTime = x.DateTime,
                    DocumentName = x.DocumentName
                }).ToArray()
            };

            await _emailSender.SendEmailAsync(partnerId, email, msgData);
        }

        public async Task SendRequestForDocument(string partnerId, string email, string fullName, string text, string comment)
        {
            var masgData = new RequestForDocumentData
            {
                FullName = fullName,
                Text = text,
                Comment = comment,
                Year = DateTime.Now.Year.ToString()
            };

            await _emailSender.SendEmailAsync(partnerId, email, masgData);
        }

        public async Task SendSwiftCashoutProcessedEmail(string partnerId, string email, string fullname, double amount, string assetId, Swift data)
        {
            var msgData = new SwiftCashoutProcessedData
            {
                FullName = fullname
            };

            await _emailSender.SendEmailAsync(partnerId, email, msgData);
        }

        public async Task SendSwiftCashoutDeclinedEmail(string partnerId, string email, string fullname, string text, string comment)
        {
            await _emailSender.SendEmailAsync(partnerId, email, new SwiftCashoutDeclinedData
            {
                FullName = fullname,
                Text = text,
                Comment = comment
            });
        }

    }
}
