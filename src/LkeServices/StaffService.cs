using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.Staff;
using JetBrains.Annotations;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PayAuth.Client.Models;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayInvoice.Client.Models.Employee;

namespace LkeServices
{
    public class StaffService : IStaffService
    {
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly IEmailPartnerRouterClient _emailPartnerRouterClient;

        public StaffService(
            [NotNull] IPayAuthClient payAuthClient, 
            [NotNull] IPayInvoiceClient payInvoiceClient, 
            [NotNull] IEmailPartnerRouterClient emailPartnerRouterClient)
        {
            _payAuthClient = payAuthClient ?? throw new ArgumentNullException(nameof(payAuthClient));
            _payInvoiceClient = payInvoiceClient ?? throw new ArgumentNullException(nameof(payInvoiceClient));
            _emailPartnerRouterClient = emailPartnerRouterClient ?? throw new ArgumentNullException(nameof(emailPartnerRouterClient));
        }

        public async Task AddAsync(NewStaffCommand cmd)
        {
            try
            {
                EmployeeModel newEmployee = await _payInvoiceClient.AddEmployeeAsync(new CreateEmployeeModel
                {
                    Email = cmd.Email,
                    LastName = cmd.LastName,
                    FirstName = cmd.FirstName,
                    MerchantId = cmd.MerchantId
                });

                await _payAuthClient.RegisterAsync(new RegisterModel
                {
                    Email = newEmployee.Email,
                    Password = cmd.Password,
                    EmployeeId = newEmployee.Id,
                    MerchantId = newEmployee.MerchantId
                });

                ResetPasswordTokenModel resetPasswordToken = await _payAuthClient.CreateResetPasswordTokenAsync(
                    new CreateResetPasswordTokenRequest
                    {
                        EmployeeId = newEmployee.Id,
                        MerchantId = newEmployee.MerchantId
                    });

                await _emailPartnerRouterClient.Send(new SendEmailCommand
                {
                    EmailAddresses = new[] { newEmployee.Email },
                    Template = "PasswordResetTemplate",
                    Payload = new Dictionary<string, string>()
                    //todo
                });
            }
            catch (Lykke.Service.PayInvoice.Client.ErrorResponseException e)
            {
                throw new AddStaffException(e.Message);
            }
            catch (Lykke.Service.PayAuth.Client.ErrorResponseException e)
            {
                throw new AddStaffException(e.Message);
            }
        }

        public async Task UpdateAsync(UpdateStaffCommand cmd)
        {
            try
            {
                EmployeeModel existingEmployee = await _payInvoiceClient.GetEmployeeAsync(cmd.EmployeeId);

                await _payInvoiceClient.UpdateEmployeeAsync(new UpdateEmployeeModel
                {
                    Id = existingEmployee.Id,
                    Email = existingEmployee.Email,
                    MerchantId = existingEmployee.MerchantId,
                    FirstName = cmd.FirstName,
                    LastName = cmd.LastName,
                    IsBlocked = cmd.IsBlocked
                });

                if (!string.IsNullOrEmpty(cmd.Password))
                {
                    await _payAuthClient.UpdateAsync(new UpdateCredentialsModel
                    {
                        Email = existingEmployee.Email,
                        EmployeeId = existingEmployee.Id,
                        MerchantId = existingEmployee.MerchantId,
                        Password = cmd.Password
                    });

                    //todo: send email PasswordResetTemplate ?
                }
            }
            catch (Lykke.Service.PayInvoice.Client.ErrorResponseException e)
            {
                throw new UpdateStaffException(e.Message);
            }
        }
    }
}
