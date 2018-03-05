using System.Threading.Tasks;
using Common;

namespace Core.BackgroundWorker
{
    public enum WorkType
    {
        SetEtheriumContract = 10,
        SetPin = 40,
        SetPartnerClientAccountInfo = 70,
        UpdateHashForOperations = 80,
        CheckPerson = 90
    }

    public interface IBackgroundWorkRequestProducer
    {
        Task ProduceRequest<T>(WorkType workType, T context);
    }

    #region Contexts

    public class SetEtheriumContractContext
    {
        public SetEtheriumContractContext(string clientId)
        {
            ClientId = clientId;
        }

        public string ClientId { get; set; }
    }

    public class SetReferralCodeContext
    {
        public SetReferralCodeContext(string clientId, string ip)
        {
            ClientId = clientId;
            Ip = ip;
        }

        public string ClientId { get; set; }
        public string Ip { get; set; }
    }

    public class SetPinContext
    {
        public SetPinContext(string clientId, string pin)
        {
            ClientId = clientId;
            Pin = pin;
        }

        public string ClientId { get; set; }
        public string Pin { get; set; }
    }

    public class UpdateHashForOperationsContext
    {
        public UpdateHashForOperationsContext(string cmdType, string contextData, string hash)
        {
            ContextData = contextData;
            CmdType = cmdType;
            Hash = hash;
        }

        public string ContextData { get; set; }
        public string Hash { get; set; }
        public string CmdType { get; set; }
    }

    public class SetPartnerAccountInfoWorkerContext
    {
        public string Email { get; set; }
    }

    public class CheckPersonContext
    {
        public string ClientId { get; set; }

        public CheckPersonContext(string id)
        {
            ClientId = id;
        }
    }

    public class BackgroundWorkMessage
    {
        public WorkType WorkType { get; set; }
        public string ContextJson { get; set; }
    }

    public class BackgroundWorkMessage<T> : BackgroundWorkMessage
    {
        public BackgroundWorkMessage(WorkType workType, T contextObj)
        {
            ContextJson = contextObj.ToJson();
            WorkType = workType;
        }
    }

    #endregion
}
