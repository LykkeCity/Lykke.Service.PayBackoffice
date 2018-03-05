using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.BcnReports
{
    public interface IAssetTransactionReport
    {
        string FileUrl { get;  }
        string Status { get;  }
        string AssetId { get;  }
        string LastError { get;  }
        DateTime? Started { get;  }
        DateTime? Finished { get;  }
        DateTime QueuedAt { get;  }
        bool NotFinished { get; }
    }

    public interface IAssetTransactionReportCommand
    {
        string Asset { get; }
        string Email { get; }
    }

    public class AssetTransactionReportCommand : IAssetTransactionReportCommand
    {
        public string Asset { get; set; }
        public string Email { get; set; }

        public static AssetTransactionReportCommand Create(string asset, string email)
        {
            return new AssetTransactionReportCommand
            {
                Asset = asset,
                Email = email
            };
        }
    }

    public interface IAssetTransactionCommandResult
    {
        IEnumerable<string> ErrorMessages { get;  }
        bool Success { get;  }
    }

    public interface IAssetTransactionsReportsService
    {
        Task<IEnumerable<IAssetTransactionReport>> GetAllAsync();
        Task<IAssetTransactionCommandResult> CreateReport(IAssetTransactionReportCommand reportCommand);
    }
}
