using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface IFeeOutputsStatusRepository
    {
        Task AddOrUpdate(string feesStateJson);
        Task<string> GetAsync();
    }
}
