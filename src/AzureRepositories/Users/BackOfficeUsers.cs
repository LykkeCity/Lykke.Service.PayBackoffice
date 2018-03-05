using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Users;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Users
{
    public class BackOfficeUserEntity : TableEntity, IBackOfficeUser
    {
        public BackOfficeUserEntity()
        {
            UseTwoFactorVerification = true;
        }

        public static string GeneratePartitionKey()
        {
            return "BackOfficeUser";
        }

        public static string GenerateRowKey(string id)
        {
            return id.Trim().ToLower();
        }

        public string Id => RowKey;
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public string Roles { get; set; }
        string[] IBackOfficeUser.Roles => Roles.FromStringViaSeparator('|').ToArray();
        public bool IsForceLogoutRequested { get; set; }
        public bool IsDisabled { get; set; }
        public string GoogleAuthenticatorPrivateKey { get; set; }
        public bool GoogleAuthenticatorConfirmBinding { get; set; }
        public bool UseTwoFactorVerification { get; set; }
        public string TwoFactorVerificationTrustedTimeSpan { get; set; }
        public string TwoFactorVerificationTrustedIPs { get; set; }

        private void SetRoles(IEnumerable<string> roles)
        {
            Roles = roles.ToStringViaSeparator("|");
        }
        
        public void Edit(string fullName, bool isAdmin, string[] roles)
        {
            IsAdmin = isAdmin;
            FullName = fullName;

            SetRoles(roles);
        }

        public static BackOfficeUserEntity Create(IBackOfficeUser src)
        {
            var result = new BackOfficeUserEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.Id)
            };

            result.Edit(src.FullName, src.IsAdmin, src.Roles);

            return result;
        }

        public void ForceLogout()
        {
            TwoFactorVerificationTrustedIPs = "";
            IsForceLogoutRequested = true;
        }

        public void ResetForceLogout()
        {
            IsForceLogoutRequested = false;
        }

        public void Enable()
        {
            IsDisabled = false;
        }

        public void Disable()
        {
            ForceLogout();

            IsDisabled = true;
        }

        public void GenerateGoogleAuthenticatorPrivateKey()
        {
            GoogleAuthenticatorPrivateKey = Guid.NewGuid() + "_" + Id;
        }

        public void ResetGoogleAuthenticatorPrivateKey()
        {
            GoogleAuthenticatorPrivateKey = "";
            GoogleAuthenticatorConfirmBinding = false;
        }

        public void SetGoogleAuthenticatorConfirmBinding(bool bindingState)
        {
            GoogleAuthenticatorConfirmBinding = bindingState;
        }

        public void SetUseTwoFactorVerification(bool state)
        {
            UseTwoFactorVerification = state;
        }

        public static BackOfficeUserEntity Create(string id, string fullName, bool isAdmin, string[] roles)
        {
            var entity = new BackOfficeUserEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(id),
                FullName = fullName,
                IsAdmin = isAdmin,
                GoogleAuthenticatorPrivateKey = null
            };

            entity.SetRoles(roles);

            return entity;
        }
    }
    
    public class BackOfficeUsersRepository : IBackOfficeUsersRepository
    {
        private readonly INoSQLTableStorage<BackOfficeUserEntity> _tableStorage;

        public BackOfficeUsersRepository(INoSQLTableStorage<BackOfficeUserEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task CreateAsync(string id, string fullName, bool isAdmin, string[] roles)
        {
            var newUser = BackOfficeUserEntity.Create(id, fullName, isAdmin, roles);
            return _tableStorage.InsertOrReplaceAsync(newUser);
        }

        public Task UpdateAsync(string id, string fullName, bool isAdmin, string[] roles)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.Edit(fullName, isAdmin, roles);
                return entity;
            });
        }

        public async Task<IBackOfficeUser> GetAsync(string id)
        {
            if (id == null)
                return null;

            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<bool> UserExists(string id)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

           return (await _tableStorage.GetDataAsync(partitionKey, rowKey)) != null;
        }

        public async Task<IEnumerable<IBackOfficeUser>> GetAllAsync()
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public Task ForceLogoutAsync(string id)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.ForceLogout();
                return entity;
            });
        }

        public Task ResetForceLogoutAsync(string id)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.ResetForceLogout();
                return entity;
            });
        }

        public Task RemoveAsync(string id)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.DeleteIfExistAsync(partitionKey, rowKey);
        }

        public Task DisableAsync(string id)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.Disable();
                return entity;
            });
        }

        public Task EnableAsync(string id)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.Enable();
                return entity;
            });
        }

        public Task GenerateGoogleAuthenticatorPrivateKey(string id)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.GenerateGoogleAuthenticatorPrivateKey();
                return entity;
            });
        }

        public Task ResetGoogleAuthenticatorPrivateKey(string id)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.ResetGoogleAuthenticatorPrivateKey();
                return entity;
            });
        }

        public Task SetGoogleAuthenticatorConfirmBinding(string id, bool bindingState)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.SetGoogleAuthenticatorConfirmBinding(bindingState);
                return entity;
            });
        }

        public Task SetUseTwoFactorVerification(string id, bool state)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.SetUseTwoFactorVerification(state);
                return entity;
            });
        }

        public Task SetUseTwoFactorVerificationTrustedTimeSpan(string id, string timeSpan)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.TwoFactorVerificationTrustedTimeSpan = timeSpan;
                return entity;
            });
        }

        public Task SetTrustedIpAddresses(string id, string value)
        {
            var partitionKey = BackOfficeUserEntity.GeneratePartitionKey();
            var rowKey = BackOfficeUserEntity.GenerateRowKey(id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.TwoFactorVerificationTrustedIPs = value;
                return entity;
            });
        }
    }
}
