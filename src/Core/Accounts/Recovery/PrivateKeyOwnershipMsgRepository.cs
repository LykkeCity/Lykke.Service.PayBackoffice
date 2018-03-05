using System.Threading.Tasks;

namespace Core.Accounts.Recovery
{
    public interface IPrivateKeyOwnershipMsgRepository
    {
        Task<string> GenerateMsgForEmail(string partnerId, string email);
        Task<string> GetMsgForEmail(string partnerId, string email);
        Task RemoveMsg(string partnerId, string email);
    }
}
