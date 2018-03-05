using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Regulator;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Regulator
{
    public class RegulatorEntity : TableEntity, IRegulator
    {
        public string InternalId { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string Countries { get; set; }
        public string TermsOfUseUrl { get; set; }
        public string CreditVoucherUrl { get; set; }
        public string MarginTradingConditions { get; set; }
        public string RiskDescriptionUrl { get; set; }

        public static string GeneratePartition()
        {
            return "Regulator";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public static RegulatorEntity Create(IRegulator regulator)
        {
            return new RegulatorEntity
            {
                PartitionKey = GeneratePartition(),
                RowKey = GenerateRowKey(regulator.InternalId),
                InternalId = regulator.InternalId,
                Name = regulator.Name,
                IsDefault = regulator.IsDefault,
                Countries = regulator.Countries,
                TermsOfUseUrl = regulator.TermsOfUseUrl,
                MarginTradingConditions = regulator.MarginTradingConditions,
                CreditVoucherUrl = regulator.CreditVoucherUrl,
                RiskDescriptionUrl = regulator.RiskDescriptionUrl
            };
        }

        public static RegulatorEntity Update(RegulatorEntity from, IRegulator to)
        {
            from.Name = to.Name;
            from.InternalId = to.InternalId;
            from.IsDefault = to.IsDefault;
            from.Countries = to.Countries;
            from.TermsOfUseUrl = to.TermsOfUseUrl;
            from.MarginTradingConditions = to.MarginTradingConditions;
            from.CreditVoucherUrl = to.CreditVoucherUrl;
            from.RiskDescriptionUrl = to.RiskDescriptionUrl;
            return Create(from);
        }
    }

    public class RegulatorRepository : IRegulatorRepository
    {
        private readonly INoSQLTableStorage<RegulatorEntity> _tableStorage;

        public RegulatorRepository(INoSQLTableStorage<RegulatorEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task CreateAsync(IRegulator regulator)
        {
            var entity = RegulatorEntity.Create(regulator);
            return _tableStorage.InsertAsync(entity);
        }

        public async Task UpdateAsync(IRegulator regulator)
        {
            var entity =
                    await
                        _tableStorage.GetDataAsync(RegulatorEntity.GeneratePartition(),
                            RegulatorEntity.GenerateRowKey(regulator.InternalId));

            RegulatorEntity.Update(entity, regulator);

            await _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task CreateOrUpdateAsync(IRegulator regulator)
        {
            var entity = await _tableStorage
                .GetDataAsync(RegulatorEntity.GeneratePartition(),
                    RegulatorEntity.GenerateRowKey(regulator.InternalId));

            if (entity == null)
                entity = RegulatorEntity.Create(regulator);
            else
                RegulatorEntity.Update(entity, regulator);

            await _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task<IEnumerable<IRegulator>> GetRegulatorsAsync()
        {
            return await _tableStorage.GetDataAsync(RegulatorEntity.GeneratePartition());
        }

        public async Task<IDictionary<string, string>> GetRegulatorsNamesAsync()
        {
            var regulators = await _tableStorage.GetDataAsync(RegulatorEntity.GeneratePartition());
            return regulators.ToDictionary(r => r.InternalId, r => r.Name);
        }

        public async Task<IRegulator> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            return await _tableStorage.GetDataAsync(RegulatorEntity.GeneratePartition(), RegulatorEntity.GenerateRowKey(id));
        }

        public async Task<IRegulator> GetByIdOrDefaultAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var regulator = await _tableStorage.GetDataAsync(RegulatorEntity.GeneratePartition(), RegulatorEntity.GenerateRowKey(id));

                if (regulator != null)
                    return regulator;
            }

            var allRegulators = await _tableStorage.GetDataAsync(RegulatorEntity.GeneratePartition());
            var defaultRegulators = allRegulators.Where(r => r.IsDefault).ToArray();

            if (defaultRegulators.Length == 1)
                return defaultRegulators.Single();

            return null;
        }

        public Task RemoveAsync(string id)
        {
            return _tableStorage.DeleteAsync(RegulatorEntity.GeneratePartition(), RegulatorEntity.GenerateRowKey(id));
        }
    }
}
