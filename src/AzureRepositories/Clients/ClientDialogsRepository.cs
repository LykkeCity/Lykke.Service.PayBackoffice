using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    //ToDo:to remove
    public class TestSubmit : TableEntity
    {
    }

    public class ClientDialogEntity : TableEntity, IClientDialog
    {
        public static string GeneratePartition(string clientId)
        {
            return clientId;
        }

        public static string GenerateRowKey(Guid dialogId)
        {
            return dialogId.ToString();
        }

        public static ClientDialogEntity Create(IClientDialog dialog)
        {
            return new ClientDialogEntity
            {
                Buttons = dialog.Buttons,
                Id = dialog.Id,
                Caption = dialog.Caption,
                Text = dialog.Text,
                Type = dialog.Type,
                PartitionKey = GeneratePartition(dialog.ClientId),
                ClientId = dialog.ClientId,
                RowKey = GenerateRowKey(dialog.Id)
            };
        }

        public Guid Id { get; set; }

        public DialogType Type
        {
            get
            {
                return Enum.Parse<DialogType>(TypeValue);
            }
            set { TypeValue = value.ToString(); }
        }
        public string TypeValue { get; set; }

        public string Caption { get; set; }
        public string Text { get; set; }

        public ClientDialogButton[] Buttons
        {
            get { return ButtonsJson.DeserializeJson<ClientDialogButton[]>(); }
            set { ButtonsJson = value.ToJson(); }
        }
        public string ButtonsJson { get; set; }

        public string ClientId { get; set; }
    }

    public class ClientDialogsRepository : IClientDialogsRepository
    {
        private readonly INoSQLTableStorage<ClientDialogEntity> _tableStorage;
        private readonly INoSQLTableStorage<TestSubmit> _testSubmitStorage; //ToDo:remove

        public ClientDialogsRepository(INoSQLTableStorage<ClientDialogEntity> tableStorage,
            INoSQLTableStorage<TestSubmit> testSubmitStorage)
        {
            _tableStorage = tableStorage;
            _testSubmitStorage = testSubmitStorage;
        }

        public Task AddDialogAsync(IClientDialog clientDialog)
        {
            var entity = ClientDialogEntity.Create(clientDialog);
            return _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task SubmitDialogAsync(string clientId, Guid dialogId, Guid buttonId)
        {
            await _tableStorage.DeleteIfExistAsync(ClientDialogEntity.GeneratePartition(clientId),
                ClientDialogEntity.GenerateRowKey(dialogId));

            await _testSubmitStorage.InsertOrReplaceAsync(new TestSubmit
            {
                PartitionKey = clientId,
                RowKey = GetSubmittedKey(dialogId, buttonId)
            });
        }

        public async Task DeleteDialogAsync(string clientId, Guid dialogId)
        {
            await _tableStorage.DeleteIfExistAsync(ClientDialogEntity.GeneratePartition(clientId), ClientDialogEntity.GenerateRowKey(dialogId));
        }

        public async Task<IClientDialog> GetDialogAsync(string clientId, Guid dialogId)
        {
            return await _tableStorage.GetDataAsync(ClientDialogEntity.GeneratePartition(clientId),
                ClientDialogEntity.GenerateRowKey(dialogId));
        }

        public async Task<IEnumerable<IClientDialog>> GetDialogsAsync(string clientId)
        {
            var dialogs = await _tableStorage.GetDataAsync(ClientDialogEntity.GeneratePartition(clientId));
            return dialogs;
        }

        public async Task<bool> IsSubmitted(string clientId, Guid dialogId, Guid buttonId)
        {
            return await _testSubmitStorage.GetDataAsync(clientId, GetSubmittedKey(dialogId, buttonId)) != null;
        }

        private string GetSubmittedKey(Guid dialogId, Guid buttonId)
        {
            return $"{dialogId}_{buttonId}";
        }
    }
}
