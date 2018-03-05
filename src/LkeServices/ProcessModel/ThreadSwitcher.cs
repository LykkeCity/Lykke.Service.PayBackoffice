using System;
using System.Threading.Tasks;

namespace LkeServices.ProcessModel
{
    public class ThreadSwitcher : IThreadSwitcher
    {
        public void SwitchThread(Func<Task> taskProc)
        {
            Task.Run(taskProc);
        }
    }
}