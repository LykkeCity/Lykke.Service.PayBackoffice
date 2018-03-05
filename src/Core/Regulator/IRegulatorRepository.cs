using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Regulator
{
    public interface IRegulator
    {
        string Name { get; }
        string InternalId { get; }
        bool IsDefault { get; }
        string Countries { get; }
        string CreditVoucherUrl { get; }
        string MarginTradingConditions { get; }
        string TermsOfUseUrl { get; set; }
        string RiskDescriptionUrl { get; set; }
    }

    public class Regulator : IRegulator
    {
        public string InternalId { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string Countries { get; set; }
        public string TermsOfUseUrl { get; set; }
        public string CreditVoucherUrl { get; set; }
        public string MarginTradingConditions { get; set; }
        public string RiskDescriptionUrl { get; set; }
    }
    
    public interface IRegulatorRepository
    {
        Task CreateAsync(IRegulator regulator);
        Task UpdateAsync(IRegulator regulator);
        Task CreateOrUpdateAsync(IRegulator regulator);
        Task<IEnumerable<IRegulator>> GetRegulatorsAsync();
        Task<IRegulator> GetAsync(string id);
        Task RemoveAsync(string id);
        Task<IRegulator> GetByIdOrDefaultAsync(string id);
        Task<IDictionary<string, string>> GetRegulatorsNamesAsync();
    }
}
