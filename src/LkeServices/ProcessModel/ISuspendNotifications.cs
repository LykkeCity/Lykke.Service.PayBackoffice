using System;

namespace Core.ProcessModel
{
    public interface ISuspendNotifications
    {
        IDisposable SuspendNotifications();
    }
}
