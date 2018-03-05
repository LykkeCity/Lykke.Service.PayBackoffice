using Core.Bitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Ethereum.Models
{
    public class GetBalanceModel
    {
        public string Address { get; set; }
        public IEnumerable<IBalanceRecord> Balances { get; set; }
    }
}
