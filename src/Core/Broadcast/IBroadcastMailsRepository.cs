using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Messages.Email;

namespace Core.Broadcast
{

    public interface IBroadcastMail
    {
        string Email { get;}
        BroadcastGroup Group { get; }
    }

    public class BroadcastMail : IBroadcastMail
    {
        public string Email { get; set; }
        public BroadcastGroup Group { get; set; }
    }

    public interface IBroadcastMailsRepository
    {
        Task SaveAsync(IBroadcastMail broadcastMail);
        Task<IEnumerable<IBroadcastMail>> GetEmailsByGroup(BroadcastGroup group);
        Task DeleteAsync(IBroadcastMail broadcastMail);
        Task DeleteAsync(BroadcastGroup group, string email);
        bool RecordAlreadyExists(IBroadcastMail broadcastMail);
    }
}
