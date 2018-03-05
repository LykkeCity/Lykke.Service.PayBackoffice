using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Offchain
{
    public interface IOffchainFinalizeCommandProducer
    {
        Task ProduceFinalize(string transferId, string clientId, string hash);
    }
}
