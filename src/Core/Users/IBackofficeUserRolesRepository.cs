using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Users
{

    public interface IBackofficeUserRole
    {
        string Id { get; }
        string Name { get;}
        UserFeatureAccess[] Features { get; }
    }

    public class BackofficeUserRole : IBackofficeUserRole
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public UserFeatureAccess[] Features { get; set; }


        public static BackofficeUserRole Create(IBackofficeUserRole src)
        {
            return new BackofficeUserRole
            {
                Id = src.Id,
                Features = src.Features,
                Name = src.Name
            };
        }

        public static readonly BackofficeUserRole Default = new BackofficeUserRole
        {
            Features = new UserFeatureAccess[0]
        };
    }

    public interface IBackofficeUserRolesRepository
    {
        Task<IEnumerable<IBackofficeUserRole>> GetAllRolesAsync();

        Task<IBackofficeUserRole> GetAsync(string id);
        Task SaveAsync(IBackofficeUserRole data);

    }
}
