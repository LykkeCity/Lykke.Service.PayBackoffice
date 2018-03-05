using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ethereum
{
    public interface IEthererumPendingActionsRepository
    {
        Task<IEnumerable<string>> GetPendingAsync(string clientId);

        Task CreateAsync(string clientId, string operationId);

        Task CompleteAsync(string clientId, string operationId);
    }
}
