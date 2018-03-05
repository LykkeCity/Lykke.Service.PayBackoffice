using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Clients;
using Core.EventLogs;
using Core.Messages.Sms;
using Core.Settings;
using Core.Sms.MessagesData;
using Core.VerificationCode;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.ClientAccount.Client;

namespace LkeServices.Clients
{
    public class ConfirmationCodesService : IConfirmationCodesService
    {
        private readonly ISmsVerificationCodeRepository _smsVerificationCodeRepository;
        private readonly ISmsRequestProducer _smsRequestProducer;        
        private readonly SupportToolsSettings _supportToolsSettings;
        private readonly ICallTimeLimitsRepository _callTimeLimitsRepository;
        private readonly IClientAccountClient _clientAccountService;
        private readonly DeploymentSettings _deploymentSettings;
        
        private const string GetMethodName = "ConfirmationCodesService.RequestSmsCode";
        private readonly TimeSpan _repeatCallsTimeSpan = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _repeatEnabledTimeSpan = TimeSpan.FromSeconds(45);

        public ConfirmationCodesService(ISmsVerificationCodeRepository smsVerificationCodeRepository,
            ISmsRequestProducer smsRequestProducer,
            ICallTimeLimitsRepository callTimeLimitsRepository,
            IClientAccountClient clientAccountService, 
			DeploymentSettings deploymentSettings, 
            SupportToolsSettings supportToolsSettings)
        {
            _smsVerificationCodeRepository = smsVerificationCodeRepository;
            _smsRequestProducer = smsRequestProducer;            

            _callTimeLimitsRepository = callTimeLimitsRepository;
            _clientAccountService = clientAccountService;
            _deploymentSettings = deploymentSettings;            
            _supportToolsSettings = supportToolsSettings;
        }

        public async Task<string> RequestSmsCode(string partnerId, string clientId, string phoneNumber, bool createPriorityCode = false)
        {
            var callHistory =
                await _callTimeLimitsRepository.GetCallHistoryAsync(GetMethodName, clientId, _repeatCallsTimeSpan);

            if (!callHistory.Any() || callHistory.IsCallEnabled(_repeatEnabledTimeSpan, 2))
            {
                await _callTimeLimitsRepository.InsertRecordAsync(GetMethodName, clientId);

                var smsSettings = await _clientAccountService.GetSmsAsync(clientId);

                //if there were some precedent calls in prev. 5 mins - we should try another SMS provider
                bool useAlternativeProvider = callHistory.Any()
                    ? !smsSettings.UseAlternativeProvider
                    : smsSettings.UseAlternativeProvider;

                await _clientAccountService.SetSmsAsync(clientId, useAlternativeProvider);

                ISmsVerificationCode smsCode;
                if (createPriorityCode)
                {
                    var expDate = DateTime.UtcNow.AddSeconds(_supportToolsSettings.PriorityCodeExpirationInterval);
                    smsCode = await _smsVerificationCodeRepository.CreatePriorityAsync(partnerId, phoneNumber, expDate);
                }
                else
                {
                    smsCode = await _smsVerificationCodeRepository.CreateAsync(partnerId, phoneNumber,
                        _deploymentSettings.IsProduction);
                }

                await _smsRequestProducer.SendSmsAsync(partnerId, phoneNumber,
                        new SmsConfirmationData { ConfirmationCode = smsCode.Code },
                        smsSettings.UseAlternativeProvider);
                return smsCode.Code;
            }

            return null;
        }

        public async Task<string> RequestSmsCode(string partnerId, string phoneNumber, bool createPriorityCode = false)
        {
            ISmsVerificationCode smsCode;
            if (createPriorityCode)
            {
                var expDate = DateTime.UtcNow.AddSeconds(_supportToolsSettings.PriorityCodeExpirationInterval);
                smsCode = await _smsVerificationCodeRepository.CreatePriorityAsync(partnerId, phoneNumber, expDate);
            }
            else
            {
                smsCode = await _smsVerificationCodeRepository.CreateAsync(partnerId, phoneNumber,
                    _deploymentSettings.IsProduction);
            }

            await _smsRequestProducer.SendSmsAsync(partnerId, phoneNumber,
                new SmsConfirmationData {ConfirmationCode = smsCode.Code}, false);

            return smsCode.Code;
        }
    }
}
