using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.BcnReports
{
    public interface IAddressTransactionReport
    {
        string FileUrl { get;  }
        string Status { get;  }
        string Address { get;  }
        string LastError { get;  }
        DateTime? Started { get;  }
        DateTime? Finished { get;  }
        DateTime QueuedAt { get;  }
        bool NotFinished { get; }
    }

    public interface IAddressTransactionReportCommand
    {
        string BcnAddress { get; }
        string Email { get; }
    }

    public class AddressTransactionReportCommand : IAddressTransactionReportCommand
    {
        public string BcnAddress { get; set; }
        public string Email { get; set; }

        public static AddressTransactionReportCommand Create(string bcnAddress, string email)
        {
            return new AddressTransactionReportCommand
            {
                BcnAddress = bcnAddress,
                Email = email
            };
        }
    }

    public interface IAddressTransactionCommandResult
    {
        IEnumerable<string> ErrorMessages { get;  }
        bool Success { get;  }
    }

    public interface IAddressTransactionsReportsService
    {
        Task<IEnumerable<IAddressTransactionReport>> GetAllAsync();
        Task<IAddressTransactionCommandResult> CreateReport(IAddressTransactionReportCommand reportCommand);
    }
}
