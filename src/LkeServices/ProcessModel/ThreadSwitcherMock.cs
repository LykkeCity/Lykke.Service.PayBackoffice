using System;
using System.Threading.Tasks;

namespace LkeServices.ProcessModel
{
    public class ThreadSwitcherMock : IThreadSwitcher
    {
        public void SwitchThread(Func<Task> taskProc)
        {
            taskProc().Wait();
        }
    }
}