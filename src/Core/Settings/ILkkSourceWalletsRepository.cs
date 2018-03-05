using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Settings
{
    public interface ILkkSourceWallet
    {
        string Address { get; }
        double StartBalance { get; }
        string Comment { get; }
    }

    public class LkkSourceWallet : ILkkSourceWallet
    {
        public string Address { get; set; }
        public double StartBalance { get; set; }
        public string Comment { get; set; }
    }

    public interface ILkkSourceWalletsRepository
    {
        Task InsertOrReplaceAsync(ILkkSourceWallet lkkSourceWallet);
        Task<ILkkSourceWallet> GetRecord(string address);
        Task<IEnumerable<ILkkSourceWallet>> GetRecordsAsync();
        Task RemoveAsync(string address);
    }
}
