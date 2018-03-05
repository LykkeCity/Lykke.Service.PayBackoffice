using System.Threading.Tasks;

namespace Core.Messages.Email
{
    public interface IVerifiedEmailsRepository
    {
        Task AddOrReplaceAsync(string email, string partnerId);
        Task<bool> IsEmailVerified(string email, string partnerId);
        Task RemoveAsync(string email, string partnerId);
        Task ChangeEmailAsync(string email, string partnerId, string newEmail);
    }
}
