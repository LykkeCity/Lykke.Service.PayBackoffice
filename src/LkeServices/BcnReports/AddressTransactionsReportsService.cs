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

    public class AddressTransactionsReportsSettings
    {
        public string BaseUri { get; set; }
    }

    #endregion

    #region ApiContracts

    public class AddressTransactionsReportCommandRequestContract
    {
        public string BitcoinAddress { get; set; }

        public string Email { get; set; }

        public static AddressTransactionsReportCommandRequestContract Create(IAddressTransactionReportCommand source)
        {
            return new AddressTransactionsReportCommandRequestContract
            {
                BitcoinAddress = source.BcnAddress,
                Email = source.Email
            };
        }
    }

    public class AddressTransactionsReportCommandResponceContract
    {
        public string[] ErrorMessages { get; set; }
        public bool Success { get; set; }
    }

    public class AddressReportMetadataViewModelContract
    {
        public string FileUrl { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string LastError { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime QueuedAt { get; set; }
    }

    #endregion

    #region ResultModels

    public class AddressTransactionReport : IAddressTransactionReport
    {
        public string FileUrl { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string LastError { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime QueuedAt { get; set; }
        public bool NotFinished => Finished == null;

        public static AddressTransactionReport Create(AddressReportMetadataViewModelContract source)
        {
            return new AddressTransactionReport
            {
                Address = source.Address,
                FileUrl = source.FileUrl,
                Finished = source.Finished,
                LastError = source.LastError,
                QueuedAt = source.QueuedAt,
                Started = source.Started,
                Status = source.Status
            };
        }
    }

    public class AddressTransactionCommandResult : IAddressTransactionCommandResult
    {
        public IEnumerable<string> ErrorMessages { get; set; }
        public bool Success { get; set; }

        public static AddressTransactionCommandResult Create(AddressTransactionsReportCommandResponceContract source)
        {
            return new AddressTransactionCommandResult
            {
                Success = source.Success,
                ErrorMessages = source.ErrorMessages ?? Enumerable.Empty<string>()
            };
        }
    }
    #endregion

    public class AddressTransactionsReportsService: IAddressTransactionsReportsService
    {
        private readonly AddressTransactionsReportsSettings _settings;

        public AddressTransactionsReportsService(AddressTransactionsReportsSettings settings)
        {
            _settings = settings;
        }

        public async Task<IEnumerable<IAddressTransactionReport>> GetAllAsync()
        {
            return (await _settings.BaseUri.AppendPathSegment("api/addresstransactionsreports")
                .GetJsonAsync<AddressReportMetadataViewModelContract[]>()).Select(AddressTransactionReport.Create);
        }

        public async Task<IAddressTransactionCommandResult> CreateReport(IAddressTransactionReportCommand reportCommand)
        {
            var result = await _settings.BaseUri.AppendPathSegment("api/addresstransactionsreports")
                .PostJsonAsync(AddressTransactionsReportCommandRequestContract.Create(reportCommand))
                .ReceiveJson<AddressTransactionsReportCommandResponceContract>();

            return AddressTransactionCommandResult.Create(result);
        }
    }
}
