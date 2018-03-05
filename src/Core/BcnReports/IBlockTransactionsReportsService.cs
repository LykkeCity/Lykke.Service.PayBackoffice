using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.BcnReports
{
    public interface IBlockTransactionReport
    {
        string FileUrl { get;  }
        string Status { get;  }
        string Block { get;  }
        string LastError { get;  }
        DateTime? Started { get;  }
        DateTime? Finished { get;  }
        DateTime QueuedAt { get;  }
        bool NotFinished { get; }
    }

    public interface IBlockTransactionReportRangeCommand
    {
        string Email { get; }
        int MinBlockHeight { get; }
        int MaxBlockHeight { get; }
    }

    public class BlockTransactionReportRangeCommand : IBlockTransactionReportRangeCommand
    {
        public string Email { get; set; }
        public int MinBlockHeight { get; set; }
        public int MaxBlockHeight { get; set; }

        public static BlockTransactionReportRangeCommand Create(int minHeight, int maxHeight, string email)
        {
            return new BlockTransactionReportRangeCommand
            {
                Email = email,
                MinBlockHeight = minHeight,
                MaxBlockHeight = maxHeight
            };
        }
    }

    public interface IBlockTransactionCommandResult
    {
        IEnumerable<string> ErrorMessages { get;  }
        bool Success { get;  }
    }

    public interface IBlockTransactionsReportsService
    {
        Task<IEnumerable<IBlockTransactionReport>> GetAllAsync();
        Task<IBlockTransactionCommandResult> CreateRangeReport(IBlockTransactionReportRangeCommand reportRangeCommand);
    }
}
