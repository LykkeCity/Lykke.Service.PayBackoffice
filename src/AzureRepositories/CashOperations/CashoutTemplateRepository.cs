using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.CashOperations;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.CashOperations
{
    public class CashoutTemplateItem : TableEntity, ICashoutTemplate
    {
        public static string GeneratePartitionKey() =>  "CashoutTemplate";
        public static string GenerateRowKey(string id) => id;

        public CashoutTemplateItem() { Id = Guid.NewGuid().ToString().ToLower(); }

        public CashoutTemplateItem(string id, CashoutTemplateType type, string name, string text)
        {
            Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString().ToLower() : id;

            PartitionKey = GeneratePartitionKey();
            RowKey = GenerateRowKey(Id);

            Type = type;
            Name = name;
            Text = text;
        }

        public string Id { get; set; }
        public CashoutTemplateType Type { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }

        public string TypeText
        {
            get { return Type.ToString(); }
            set { Type = value.ToCashoutTemplateType(); }
        }
    }

    public class CashoutTemplateRepository : ICashoutTemplateRepository
    {
        private readonly INoSQLTableStorage<CashoutTemplateItem> _tableStorage;

        public CashoutTemplateRepository(INoSQLTableStorage<CashoutTemplateItem> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task SaveTemplate(ICashoutTemplate template)
        {
            var temlateItem = new CashoutTemplateItem(template.Id, template.Type, template.Name, template.Text);
            await _tableStorage.InsertOrReplaceAsync(temlateItem);
        }

        public async Task<List<ICashoutTemplate>> GetAllTemplates()
        {
            var templateItems = await _tableStorage.GetDataAsync();
            if (templateItems == null)
                return new List<ICashoutTemplate>();

            return templateItems.Cast<ICashoutTemplate>().ToList();
        }


        public async Task<List<ICashoutTemplate>> GetTemplatesByType(CashoutTemplateType type)
        {
            var partitionKeyCond = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CashoutTemplateItem.GeneratePartitionKey());
            var typeTextCond = TableQuery.GenerateFilterCondition(nameof(CashoutTemplateItem.TypeText), QueryComparisons.Equal, type.ToString());
            var query = new TableQuery<CashoutTemplateItem>
            {
                FilterString = TableQuery.CombineFilters(partitionKeyCond, TableOperators.And, typeTextCond)
            };
            
            var templateItems = await _tableStorage.WhereAsync(query);
            
            if (templateItems == null)
                return new List<ICashoutTemplate>();

            return templateItems.Cast<ICashoutTemplate>().ToList();
        }

        public async Task<ICashoutTemplate> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return await _tableStorage.GetDataAsync(CashoutTemplateItem.GeneratePartitionKey(), CashoutTemplateItem.GenerateRowKey(id));
        }

        public async Task DeleteAsync(string id)
        {
            await _tableStorage.DeleteIfExistAsync(CashoutTemplateItem.GeneratePartitionKey(), CashoutTemplateItem.GenerateRowKey(id));
        }
    }
}
