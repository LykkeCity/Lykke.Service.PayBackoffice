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
    }
}
