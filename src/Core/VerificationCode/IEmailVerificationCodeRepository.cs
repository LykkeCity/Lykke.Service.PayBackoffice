using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.VerificationCode
{
    public interface IEmailVerificationCode
    {
        string Id { get; }
        string Email { get; }
        string Code { get; }
        DateTime CreationDateTime { get; }
    }

    public interface IEmailVerificationPriorityCode : IEmailVerificationCode
    {
        DateTime ExpirationDate { get; set; }
    }

    public class EmailEmailVerificationCode : IEmailVerificationCode
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime CreationDateTime { get; set; }
    }

    public interface IEmailVerificationCodeRepository
    {
        Task<IEmailVerificationCode> CreateAsync(string email, string partnerId, bool generateRealCode);

        Task<IEmailVerificationPriorityCode> CreatePriorityAsync(string email, string partnerId, DateTime expirationDt);

        /// <summary>
        /// Returns the latest generated code
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns></returns>
        Task<IEmailVerificationCode> GetActualCode(string email, string partnerId);

        Task<bool> CheckAsync(string email, string partnerId, string codeToCheck);

        Task DeleteCodesByEmailAsync(string email, string partnerId);
    }
}
