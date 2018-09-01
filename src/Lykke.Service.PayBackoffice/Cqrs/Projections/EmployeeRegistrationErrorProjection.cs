using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInvoice.Contract.Events;

namespace BackOffice.Cqrs.Projections
{
    public class EmployeeRegistrationErrorProjection
    {
        private readonly ILog _log;

        public EmployeeRegistrationErrorProjection([NotNull] ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        public Task Handle(EmployeeRegistrationFailedEvent evt)
        {
            _log.Critical(
                "Employee registration",
                message: $"Failed to register new employee: {evt.Error}",
                context: evt.Email);

            return Task.CompletedTask;
        }

        public Task Handle(EmployeeUpdateFailedEvent evt)
        {
            _log.Critical(
                "Employee update",
                message: $"Failed to update employee: {evt.Error}",
                context: evt.Email);

            return Task.CompletedTask;
        }
        
        // temporary
        public Task Handle(EmployeeRegisteredEvent evt)
        {
            _log.Info("Employee registered", $"Email = {evt.Email}");

            return Task.CompletedTask;
        }

        // temporary
        public Task Handle(EmployeeUpdatedEvent evt)
        {
            _log.Info("Employee updated", $"Email = {evt.Email}");

            return Task.CompletedTask;
        }
    }
}
