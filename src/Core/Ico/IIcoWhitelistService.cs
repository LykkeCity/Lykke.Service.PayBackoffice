using System.Threading.Tasks;

namespace Core.Ico
{
    public interface IIcoWhitelistService
    {
        Task<bool> IsUserWhitelisted(string email);
    }
}
