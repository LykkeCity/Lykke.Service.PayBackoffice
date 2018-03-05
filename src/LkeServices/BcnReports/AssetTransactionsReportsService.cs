using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.BcnReports;
using Flurl;
using Flurl.Http;

namespace LkeServices.BcnReports
{
    #region Settings

    public class AssetTransactionsReportsSettings
    {
        public string BaseUri { get; set; }
    }

    #endregion

    #region ApiContracts

    public class AssetTransactionsReportCommandRequestContract
    {
        public string Asset { get; set; }

        public string Email { get; set; }

        public static AssetTransactionsReportCommandRequestContract Create(IAssetTransactionReportCommand source)
        {
            return new AssetTransactionsReportCommandRequestContract
            {
                Asset = source.Asset,
                Email = source.Email
            };
        }
    }

    public class AssetTransactionsReportCommandResponceContract
    {
        public string[] ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class AssetReportMetadataViewModelContract
    {
        public string FileUrl { get; set; }
        public string Status { get; set; }
        public string AssetId { get; set; }
        public string LastError { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime QueuedAt { get; set; }
    }

    #endregion

    #region ResultModels

    public class AssetTransactionReport : IAssetTransactionReport
    {
        public string FileUrl { get; set; }
        public string Status { get; set; }
        public string AssetId { get; set; }
        public string LastError { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime QueuedAt { get; set; }
        public bool NotFinished => Finished == null;

        public static AssetTransactionReport Create(AssetReportMetadataViewModelContract source)
        {
            return new AssetTransactionReport
            {
                AssetId = source.AssetId,
                FileUrl = source.FileUrl,
                Finished = source.Finished,
                LastError = source.LastError,
                QueuedAt = source.QueuedAt,
                Started = source.Started,
                Status = source.Status
            };
        }
    }

    public class AssetTransactionCommandResult : IAssetTransactionCommandResult
    {
        public IEnumerable<string> ErrorMessages { get; set; }
        public bool Success { get; set; }

        public static AssetTransactionCommandResult Create(AssetTransactionsReportCommandResponceContract source)
        {
            return new AssetTransactionCommandResult
            {
                Success = source.Success,
                ErrorMessages = source.ErrorMessages ?? Enumerable.Empty<string>()
            };
        }
    }
    #endregion

    public class AssetTransactionsReportsService: IAssetTransactionsReportsService
    {
        private readonly AssetTransactionsReportsSettings _settings;

        public AssetTransactionsReportsService(AssetTransactionsReportsSettings settings)
        {
            _settings = settings;
        }

        public async Task<IEnumerable<IAssetTransactionReport>> GetAllAsync()
        {
            return (await _settings.BaseUri.AppendPathSegment("api/assettransactionsreports")
                .GetJsonAsync<AssetReportMetadataViewModelContract[]>()).Select(AssetTransactionReport.Create);
        }

        public async Task<IAssetTransactionCommandResult> CreateReport(IAssetTransactionReportCommand reportCommand)
        {
            var result = await _settings.BaseUri.AppendPathSegment("api/assettransactionsreports")
                .PostJsonAsync(AssetTransactionsReportCommandRequestContract.Create(reportCommand))
                .ReceiveJson<AssetTransactionsReportCommandResponceContract>();

            return AssetTransactionCommandResult.Create(result);
        }
    }
}
