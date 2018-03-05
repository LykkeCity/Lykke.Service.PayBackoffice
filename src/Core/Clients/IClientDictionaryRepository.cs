using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IKeyValue
    {
        string Key { get; set; }
        string Value { get; set; }
    }

    public class KeyValue : IKeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public interface IClientDictionaryRepository
    {
        Task SaveAsync(string clientId, IKeyValue keyValue);
        Task RemoveAsync(string clientId, string key);
        Task<IKeyValue> GetAsync(string clientId, string key);
        Task<IKeyValue[]> GetAllAsync(string clientId);
    }
}
