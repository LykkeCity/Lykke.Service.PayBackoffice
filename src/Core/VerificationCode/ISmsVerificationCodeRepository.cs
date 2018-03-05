using System;
using System.Threading.Tasks;

namespace Core.VerificationCode
{
    public interface ISmsVerificationCode
    {
        string Id { get; }
        string Phone { get; }
        string Code { get; }
        DateTime CreationDateTime { get; }
    }

    public interface ISmsVerificationPriorityCode : ISmsVerificationCode
    {
        DateTime ExpirationDate { get; set; }
    }

    public class SmsVerificationCode : ISmsVerificationCode
    {
        public string Id { get; set; }
        public string Phone { get; set; }
        public string Code { get; set; }
        public DateTime CreationDateTime { get; set; }
    }

    public interface ISmsVerificationCodeRepository
    {
        Task<ISmsVerificationCode> CreateAsync(string partnerId, string phoneNum, bool generateRealCode);

        Task<ISmsVerificationCode> CreatePriorityAsync(string partnerId, string phoneNum, DateTime expirationDt);

        /// <summary>
        /// Returns the latest generated code
        /// </summary>
        /// <param name="phoneNum">Phone number</param>
        /// <returns></returns>
        Task<ISmsVerificationCode> GetActualCode(string partnerId, string phoneNum);

        Task<bool> CheckAsync(string partnerId, string phoneNum, string codeToCheck);

        Task DeleteCodesByPhoneNumAsync(string partnerId, string phoneNum);
    }
}
