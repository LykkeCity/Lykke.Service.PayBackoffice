using System;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace Core.BackOffice
{
    public interface IUserSettingsRepository
    {
        Task<string> GetAsync(string userId, string field);
        Task SaveAsync(string userId, string field, string value);
    }

    public static class UserSettingsRepositoryExt
    {
        public static async Task<T> GetAsync<T>(this IUserSettingsRepository repository, string userId, string field, Func<T> createDefault = null)
        {
            var data = await repository.GetAsync(userId, field);
            if (data == null)
                return createDefault == null ? default(T) : createDefault();

            try
            {
                return data.DeserializeJson<T>();
            }
            catch (Exception)
            {
                return createDefault == null ? default(T) : createDefault() ;
            }
        }

        public static Task SaveAsync(this IUserSettingsRepository repository, string userId, string field, object value)
        {
            var valueAsStr = value.ToJson();
            return repository.SaveAsync(userId, field, valueAsStr);
        }
    }

    public static class UserSettings
    {
        public const string LastUserView = "LastUserView";


        private const string LastUserList = "LastUserList";

        public static async Task<string[]> GetLastSeletedUsersAsync(this IUserSettingsRepository repo, string userId)
        {
            var data = await repo.GetAsync(userId, LastUserList);
            return data.DeserializeJson(() => new string[0]);
        }

        public static async Task SetLastSeletedUsersAsync(this IUserSettingsRepository repo, string userId, string email)
        {
            if (string.IsNullOrEmpty(email))
                return;

            email = email.ToLower();

            var list = (await repo.GetLastSeletedUsersAsync(userId)).ToList();



            var index = list.IndexOf(email);
            if (index>=0)
                list.RemoveAt(index);

            while (list.Count>=20)
                list.RemoveAt(list.Count-1);

            list.Insert(0, email);

            await repo.SaveAsync(userId, LastUserList, list.ToJson());
        }

    }


}
