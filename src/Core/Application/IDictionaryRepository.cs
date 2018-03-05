using System.Threading.Tasks;
using Core.Clients;

namespace Core.Application
{
    public interface IDictionaryRepository
    {
        Task SaveAsync(IKeyValue keyValue);
        Task RemoveAsync(string key);
        Task<IKeyValue> GetAsync(string key);
        Task<IKeyValue[]> GetAllAsync();
    }
}
