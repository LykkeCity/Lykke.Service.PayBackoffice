using Common;
using Common.Log;
using Core.Kyc;
using Lykke.Service.JumioIntegration.Client;
using Lykke.Service.JumioIntegration.Client.AutorestClient.Models;
using Lykke.Service.Kyc.Abstractions.Domain.Documents;
using Lykke.Service.Kyc.Abstractions.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PersonalData.Contract;
using Microsoft.Rest;

namespace LkeServices.Kyc
{
    public interface IJumioService
    {
        void StartVerification(string clientId);
        Task<IVerification> GetVerificationResult(string clientId);
        Task<JumioFaceMatch?> GetFaceMatch(string clientId);
    }

    public class JumioService : IJumioService
    {
        private readonly JumioServiceClientSettings _settings;
        private readonly ILog _log;
        private readonly JumioIntegrationClient _client;
        private readonly IKycDocumentsService _kycDocumentsService;
        private readonly IPersonalDataService _personalDataService;

        public JumioService(
            JumioServiceClientSettings settings,
            ILog log,
            IKycDocumentsService kycDocumentsService,
            IPersonalDataService personalDataService
            )
        {
            _settings = settings;
            _log = log;
            _kycDocumentsService = kycDocumentsService;
            _personalDataService = personalDataService;

            _client = _settings.Enable ? new JumioIntegrationClient(settings.ServiceUrl, log) : null;
        }

        public async Task<IVerification> GetVerificationResult(string clientId)
        {
            if (_client == null)
            {
                return null;
            }

            try
            {
                var verification = await _client.GetVerificationResult(clientId);
                return verification;
            }
            catch (HttpOperationException ex)
            {
                if (ex.Message.Contains("NoContent"))
                    return null;

                throw;
            }
        }

        public void StartVerification(string clientId)
        {
            if (_client == null)
            {
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    var allDocs = await _kycDocumentsService.GetOneEachTypeLatestAsync(clientId);

                    var idDoc = allDocs.FirstOrDefault(doc => doc.Type == KycDocumentTypeApi.IdCard.ToText());
                    var idBackSideDoc = allDocs.FirstOrDefault(doc => doc.Type == KycDocumentTypeApi.IdCardBackSide.ToText());
                    var selfieDoc = allDocs.FirstOrDefault(doc => doc.Type == KycDocumentTypeApi.Selfie.ToText());

                    if (idDoc == null || selfieDoc == null)
                    {
                        await _log.WriteWarningAsync("JumioService", "StartVerification", (new { clientId }).ToJson(), "Cannot start Jumio verification: idDoc and/or selfieDoc is empty");
                        return;
                    }

                    var idData = await _personalDataService.GetDocumentScan(idDoc.DocumentId);
                    var idBackSideData = new byte[0];
                    if (idBackSideDoc != null)
                        idBackSideData = await _personalDataService.GetDocumentScan(idBackSideDoc.DocumentId);
                    var selfieData = await _personalDataService.GetDocumentScan(selfieDoc.DocumentId);

                    var idCardType = idDoc.IdType ?? IdCardType.Id;
                    if (idCardType == null || idCardType == IdCardType.Unknown)
                    {
                        await _log.WriteWarningAsync("JumioService", "StartVerification", (new { clientId }).ToJson(), "Cannot start Jumio verification: Type of ID is not specified");
                        return;
                    }

                    var isStarted = await _client.TryToVerifyAsync(clientId, ToModelIdType(idCardType), idData, idBackSideData, selfieData);

                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync("JumioService", "StartVerification", (new { clientId }).ToJson(), ex);
                    return;
                }

            });
        }

        public async Task<JumioFaceMatch?> GetFaceMatch(string clientId)
        {
            if (_client == null)
            {
                return null;
            }

            try
            {
                var idSelfieSimilarity = await _client.GetIdSelfieSimilarity(clientId);

                return idSelfieSimilarity?.ToDomain();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static IdType ToModelIdType(IdCardType idCardType)
        {
            switch (idCardType)
            {
                case IdCardType.Unknown: return IdType.Unknown;
                case IdCardType.Passport: return IdType.PASSPORT;
                case IdCardType.Id: return IdType.IDCARD;
                case IdCardType.DrivingLicense: return IdType.DRIVINGLICENSE;

                default: return IdType.Unknown;
            }
        }
    }

    public static class JumioServiceHelpers
    {
        public static JumioFaceMatch ToDomain(this IdSelfieSimilarity src)
        {
            switch (src)
            {
                case IdSelfieSimilarity.Match: return JumioFaceMatch.Match;
                case IdSelfieSimilarity.NoMatch: return JumioFaceMatch.NotMatch;
                case IdSelfieSimilarity.NotSure: return JumioFaceMatch.UnClear;

                default: return JumioFaceMatch.Unknown;
            }
        }
    }
}
