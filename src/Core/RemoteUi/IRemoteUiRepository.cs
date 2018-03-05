using System;
using System.Threading.Tasks;

namespace Core.RemoteUi
{

    public class RemoteUiServerSocket
    {
        public int ConnectionsCount { get; set; }
        public DateTime LastConnection { get; set; }
        public DateTime LastSendTime { get; set; }
        public DateTime LastRecieveTime { get; set; }
        public DateTime LastDisconnectionTime { get; set; }
        public long Sent { get; set; }
        public long Recieve { get; set; }


    }

    public class RemoteUiData
    {
        public RemoteUiServerSocket TcpSocket { get; set; }

        public static RemoteUiData CreateDefault()
        {
            return new RemoteUiData
            {
                TcpSocket = new RemoteUiServerSocket()
            };
        }
    }


    public interface IRemoteUiRepository
    {
        Task SaveAsync(RemoteUiData data);

        Task<RemoteUiData> GetAsync();
    }
}
