using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.BcnReports;
using Flurl;
using Flurl.Http;

namespace LkeServices.BcnReports
{
    #region Settings

    public class BlockTransactionsReportSettings
    {
        public string BaseUri { get; set; }
    }

    #endregion

    #region Contracts

    #region ApiContracts

    public class BlockTransactionsReportCommandRequestContract
    {
        public IEnumerable<string> Blocks { get; set; }

        public string Email { get; set; }

        public static BlockTransactionsReportCommandRequestContract Create(IEnumerable<string> blocks, string email)
        {
            return new BlockTransactionsReportCommandRequestContract
            {
                Blocks = blocks,
                Email = email
            };
        }
    }

    public class BlockTransactionsReportCommandResponceContract
    {
        public string[] ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class BlockReportMetadataViewModelContract
    {
        public string FileUrl { get; set; }
        public string Status { get; set; }
        public string Block { get; set; }
        public string LastError { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime QueuedAt { get; set; }
    }

    #endregion

    #region ResultModels

    public class BlockTransactionReport : IBlockTransactionReport
    {
        public string FileUrl { get; set; }
        public string Status { get; set; }
        public string Block { get; set; }
        public string LastError { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime QueuedAt { get; set; }
        public bool NotFinished => Finished == null;

        public static BlockTransactionReport Create(BlockReportMetadataViewModelContract source)
        {
            return new BlockTransactionReport
            {
                Block = source.Block,
                FileUrl = source.FileUrl,
                Finished = source.Finished,
                LastError = source.LastError,
                QueuedAt = source.QueuedAt,
                Started = source.Started,
                Status = source.Status
            };
        }
    }

    public class BlockTransactionCommandResult : IBlockTransactionCommandResult
    {
        public IEnumerable<string> ErrorMessages { get; set; }
        public bool Success { get; set; }

        public static BlockTransactionCommandResult Create(BlockTransactionsReportCommandResponceContract source)
        {
            return new BlockTransactionCommandResult
            {
                Success = source.Success,
                ErrorMessages = source.ErrorMessages ?? Enumerable.Empty<string>()
            };
        }
    }
    #endregion

    #endregion

    public class BlockTransactionsReportsService : IBlockTransactionsReportsService
    {
        private readonly BlockTransactionsReportSettings _settings;

        public BlockTransactionsReportsService(BlockTransactionsReportSettings settings)
        {
            _settings = settings;
        }

        public async Task<IEnumerable<IBlockTransactionReport>> GetAllAsync()
        {
            return (await _settings.BaseUri.AppendPathSegment("api/blocktransactionsreports")
                .GetJsonAsync<BlockReportMetadataViewModelContract[]>())
                .Select(BlockTransactionReport.Create);
        }

        public async Task<IBlockTransactionCommandResult> CreateRangeReport(IBlockTransactionReportRangeCommand reportRangeCommand)
        {
            var blockHeightsForReport = Enumerable.Range(reportRangeCommand.MinBlockHeight, reportRangeCommand.MaxBlockHeight - reportRangeCommand.MinBlockHeight + 1);


            var request =BlockTransactionsReportCommandRequestContract.Create(
                blockHeightsForReport.Select(p => p.ToString()),
                reportRangeCommand.Email);

            var result = await _settings.BaseUri.AppendPathSegment("api/blocktransactionsreports")
                .PostJsonAsync(request)
                .ReceiveJson<BlockTransactionsReportCommandResponceContract>();


            return BlockTransactionCommandResult.Create(result);
        }
    }
}
