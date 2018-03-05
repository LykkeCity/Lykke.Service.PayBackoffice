using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.CashOperations
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CashoutTemplateType
    {
        Unknown, RequestForDocs, Decline
    }

    public interface ICashoutTemplate
    {
        string Id { get; }
        CashoutTemplateType Type { get; }
        string Name { get; }
        string Text { get; }
    }

    public interface ICashoutTemplateRepository
    {
        Task SaveTemplate(ICashoutTemplate template);
        Task<List<ICashoutTemplate>> GetAllTemplates();
        Task<List<ICashoutTemplate>> GetTemplatesByType(CashoutTemplateType type);
        Task<ICashoutTemplate> GetAsync(string id);
        Task DeleteAsync(string id);
    }

    public static class CashoutTemplateTypes
    {
        public static IEnumerable<CashoutTemplateType> GetAllTypes()
        {
            yield return CashoutTemplateType.RequestForDocs;
            yield return CashoutTemplateType.Decline;
        }

        public static IEnumerable<string> GetAllTypesAsText()
        {
            return GetAllTypes().Select(t => t.ToString());
        }

        public static CashoutTemplateType ToCashoutTemplateType(this string src)
        {
            CashoutTemplateType tmplType;
            var isParsed = Enum.TryParse(src, true, out tmplType);
            if (isParsed)
                return tmplType;

            return CashoutTemplateType.Unknown;
        }
    }
}