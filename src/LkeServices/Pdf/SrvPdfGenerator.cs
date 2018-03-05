using System;
using System.IO;
using System.Threading.Tasks;
using Core.Clients;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using Core.Settings;
using Common.Log;
using Core.Pdf;
using Core.CashOperations;
using Core.CashOperations.PaymentSystems;
using Common;
using Lykke.Service.PersonalData.Contract;
using Flurl.Http;
using System.Collections.Generic;
using Lykke.Service.GoogleDriveUpload.Client;
using LkeServices.CashoutRequests;

namespace LkeServices.Pdf
{
    public class SrvPdfGenerator
    {
        private static string ContainerName => "pdfs";

        private readonly PdfGeneratorSettings _settings;
        private readonly ILog _log;
        private readonly IPdfGeneratorRepository _pdfGeneratorRepository;
        private readonly CashoutRequestsManager _cashoutRequestsManager;
        private readonly IPersonalDataService _personalDataService;
        private readonly GoogleDriveSettings _googleDriveSettings;
        private readonly IGoogleDriveUploadClient _googleDriveService;

        public SrvPdfGenerator(
            PdfGeneratorSettings settings, 
            ILog log, 
            IPdfGeneratorRepository pdfGeneratorRepository,
            CashoutRequestsManager cashoutRequestsManager,
            IPersonalDataService personalDataService,
            GoogleDriveSettings googleDriveSettings,
            IGoogleDriveUploadClient googleDriveService)
        {
            _settings = settings;
            _log = log;
            _pdfGeneratorRepository = pdfGeneratorRepository;
            _cashoutRequestsManager = cashoutRequestsManager;
            _personalDataService = personalDataService;
            _googleDriveSettings = googleDriveSettings;
            _googleDriveService = googleDriveService;
        }

        private static string SwiftDetailsBlobName(string clientId, string requestId) => $"{clientId}_{requestId}.pdf";
        
        public void GenerateFromSwiftDetails(ICashOutRequest request, string initialRequestId, DateTime dateOfTransaction)
        {
            Task.Run(async () => {
                if (request == null || string.IsNullOrWhiteSpace(initialRequestId))
                    return;

                var swiftFields = request.PaymentFields.DeserializeJson<Swift>();
                var pd = await _personalDataService.GetAsync(request.ClientId);
                
                var htmlContent = "<!DOCTYPE html><html><head><title>Cash out request</title></head><body><div style=\"font-family: Geneva, Arial, Helvetica, sans-serif; width: 650px; margin:20px auto;\"><div style=\"width:650px;\"><img src=\"https://lkefiles.blob.core.windows.net/images/emails/logo_emails.png\" height=\"40\" alt=\"Lykke logo\" /></div><table style=\"margin:auto;width:650px;border: none;background-color: #fff;\"><tr><td style=\"text-align:center;padding:0 40px 0 40px;\"><img src=\"https://lkefiles.blob.core.windows.net:443/images/emails/withdraw-icon.png\" width=\"200\" alt=\"Letter image\" /></td></tr><tr><td style=\"text-align:center;\"><span style=\"font-size: 1.4em; font-weight: bold;\">Cash Out Request <br/>(@[Amount] @[Asset] for @[UserEmail]) <br/>Processed</span></td></tr><tr><td style=\"padding:20px 40px 60px 40px;\"><table style=\"border-collapse:collapse\"><tr style=\"border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;\"><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #8C94A0;\">SWIFT / ABA Routing</span></td><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #3F4D60;\">@[Bic]</span></td></tr><tr style=\"border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;\"><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #8C94A0;\">Name of the bank</span></td><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #3F4D60;\">@[BankName]</span></td></tr><tr style=\"border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;\"><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #8C94A0;\">Beneficiary's Account number (IBAN)</span></td><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #3F4D60;\">@[AccNumber]</span></td></tr><tr style=\"border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;\"><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #8C94A0;\">Name of the account holder</span></td><td style=\"word-break:break-all; padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #3F4D60;\">@[AccName]</span></td></tr><tr style=\"border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;\"><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #8C94A0;\">Country of the account holder</span></td><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #3F4D60;\">@[AccHolderCountry]</span></td></tr><tr style=\"border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;\"><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #8C94A0;\">Zip Code of the account holder</span></td><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #3F4D60;\">@[AccHolderZipCode]</span></td></tr><tr style=\"border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;\"><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #8C94A0;\">City of the account holder</span></td><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #3F4D60;\">@[AccHolderCity]</span></td></tr><tr style=\"border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;\"><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #8C94A0;\">Address of the account holder</span></td><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #3F4D60;\">@[AccHolderAddress]</span></td></tr><tr style=\"border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;\"><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #8C94A0;\">Date of the transaction</span><br/><span style=\"font-size: 1.1em;color: #8C94A0;\">(Year/Month/Date)</span></td><td style=\"padding:15px 0 15px 0;\" width=\"260\"><span style=\"font-size: 1.1em;color: #3F4D60;\">@[DateOfTransaction]</span></td></tr></table></td></tr></table></div></body></html>";

                htmlContent = htmlContent.Replace("@[Amount]", request.Amount.ToString());
                htmlContent = htmlContent.Replace("@[Asset]", request.AssetId);
                htmlContent = htmlContent.Replace("@[UserEmail]", pd.Email);

                htmlContent = htmlContent.Replace("@[Bic]", swiftFields.Bic);
                htmlContent = htmlContent.Replace("@[BankName]", swiftFields.BankName);
                htmlContent = htmlContent.Replace("@[AccNumber]", swiftFields.AccNumber);
                htmlContent = htmlContent.Replace("@[AccName]", swiftFields.AccName);
                htmlContent = htmlContent.Replace("@[AccHolderCountry]", swiftFields.AccHolderCountry);
                htmlContent = htmlContent.Replace("@[AccHolderZipCode]", swiftFields.AccHolderZipCode);
                htmlContent = htmlContent.Replace("@[AccHolderCity]", swiftFields.AccHolderCity);
                htmlContent = htmlContent.Replace("@[AccHolderAddress]", swiftFields.AccHolderAddress);
                htmlContent = htmlContent.Replace("@[DateOfTransaction]", dateOfTransaction.ToString("yyyy/MM/dd"));

                await HtmlToPdfAsync(htmlContent, SwiftDetailsBlobName(request.ClientId, initialRequestId), $"{dateOfTransaction:yyyyMMdd}_withdrawal {pd.FullName}.pdf");
                await SaveSwiftPdfReportToGoogleDriveAsync(request.ClientId, initialRequestId, request.AssetId);
            });
        }
        
        private async Task SaveSwiftPdfReportToGoogleDriveAsync(string clientId, string requestId, string assetId)
        {
            byte[] fileData = null;
            string fileName = null;

            var breakTime = DateTime.Now.AddMinutes(3);
            while (true)
            {
                if (DateTime.Now > breakTime)
                    break;

                (fileData, fileName) = await GetSwiftDetails(clientId, requestId);
                if (!string.IsNullOrEmpty(fileName))
                    break;
            }

            if (fileData == null || fileName == null)
            {
                await _log.WriteWarningAsync("SrvPdfGenerator", nameof(SaveSwiftPdfReportToGoogleDriveAsync), (new { clientId, requestId, assetId }).ToJson(), "fileData is empty");
                return;
            }

            var parentFolderGoogleId = GetFolderIdByAsset(assetId);
            if (parentFolderGoogleId == null)
            {
                await _log.WriteWarningAsync("SrvPdfGenerator", nameof(SaveSwiftPdfReportToGoogleDriveAsync), (new { clientId, requestId, assetId }).ToJson(), "parentFolderGoogleId is empty");
                return;
            }

            var fileId = await _googleDriveService.UploadFileAsync(fileName, fileData, parentFolderGoogleId);
        }

        private string GetFolderIdByAsset(string assetId)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                return _googleDriveSettings.OthersFolderId;

            string parentFolderGoogleId = null;
            switch (assetId.ToUpper())
            {
                case "USD":
                    parentFolderGoogleId = _googleDriveSettings.UsdFolderId;
                    break;
                case "EUR":
                    parentFolderGoogleId = _googleDriveSettings.EurFolderId;
                    break;
                case "GBP":
                    parentFolderGoogleId = _googleDriveSettings.GbpFolderId;
                    break;
                case "CHF":
                    parentFolderGoogleId = _googleDriveSettings.ChfFolderId;
                    break;
                default:
                    parentFolderGoogleId = _googleDriveSettings.OthersFolderId;
                    break;
            }

            return parentFolderGoogleId;
        }
        
        public async Task<(byte[] Data, string FileName)> GetSwiftDetails(string clientId, string requestId)
        {
            var result = await GetSavedData(SwiftDetailsBlobName(clientId, requestId));
            return result;
        }

        public async Task<bool> IsSwiftDetailsReportExists(string clientId, string requestId)
        {
            var result = await _pdfGeneratorRepository.IsExists(ContainerName, SwiftDetailsBlobName(clientId, requestId));

            if (!result)
                TryReGenerateFromSwiftDetails(clientId, requestId);

            return result;
        }

        private void TryReGenerateFromSwiftDetails(string clientId, string requestId)
        {
            Task.Run(async () => 
            {
                var paymentDate = await _cashoutRequestsManager.GetPaymentDateAsync(requestId);
                if (paymentDate == null)
                    return;

                var requests = await _cashoutRequestsManager.GetRelatedRequestsAsync(requestId);
                foreach (var request in requests)
                {
                    if (string.IsNullOrEmpty(request.PreviousId))
                        continue;

                    GenerateFromSwiftDetails(request, request.PreviousId, paymentDate.Value);
                }
            });
        }

        private async Task<string> HtmlToPdfAsync(string htmlSource, string blobName = null, string fileName = null)
        {
            try
            {
                var asciiBlobName = UnicodeToAsciiString(blobName);
                var asciiFileName = UnicodeToAsciiString(fileName);

                var jsonRequestContent = JsonConvert.SerializeObject(new { HtmlSource = htmlSource, BlobName = asciiBlobName, FileName = asciiFileName });

                var _cookieContainer = new CookieContainer();
                var _httpClientHandler = new HttpClientHandler() { CookieContainer = _cookieContainer };
                var _httpClient = new HttpClient(_httpClientHandler);
                _httpClient.Timeout = TimeSpan.FromSeconds(60);

                var postBody = new StringContent(jsonRequestContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_settings.ServiceUrl, postBody);

                if (response.Content != null)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var container = obj.BlobContainer?.ToString();
                    if (container != ContainerName)
                    {
                        var exception = new Exception("Container name not supported");
                        await _log.WriteFatalErrorAsync("SrvPdfGenerator", "HtmlToPdf", string.Empty, exception);
                        return null;
                    }

                    return obj.BlobName?.ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<(byte[] Data, string FileName)> GetSavedData(string blobName)
        {
            try
            {
                var result = await _pdfGeneratorRepository.GetDataAsync(ContainerName, blobName);
                return result;
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException ex)
            {
                await _log.WriteErrorAsync("SrvPdfGenerator", nameof(GetSavedData), (new { ContainerName, blobName }).ToJson(), ex);
                return (null, null);
            }
        }

        private static string UnicodeToAsciiString(string source)
        {
            if (source == null)
                return null;

            var ascii = Encoding.ASCII;
            var unicode = Encoding.Unicode;

            var unicodeBytes = unicode.GetBytes(source);

            var asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

            var asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            var asciiString = new string(asciiChars);

            return asciiString;
        }
    }
}
