using System.Threading.Tasks;

namespace Core.Staff
{
    public interface IStaffService
    {
        Task AddAsync(NewStaffCommand cmd);

        Task UpdateAsync(UpdateStaffCommand cmd);
    }
}
