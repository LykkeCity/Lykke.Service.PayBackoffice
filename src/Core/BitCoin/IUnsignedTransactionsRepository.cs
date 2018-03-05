using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface IUnsignedTransaction
    {
        string Id { get; set; }
        string Hex { get; set; }
        string ClientId { get; set; }
    }

    public class UnsignedTransaction : IUnsignedTransaction
    {
        public string Id { get; set; }
        public string Hex { get; set; }
        public string ClientId { get; set; }
    }

    public interface IUnsignedTransactionsRepository
    {
        Task<IEnumerable<IUnsignedTransaction>> GetPendingTransactionsAsync(string clientId);
        Task InsertAsync(IUnsignedTransaction transaction);
        Task RemoveAsync(string clientId, string transactionId);
    }
}
