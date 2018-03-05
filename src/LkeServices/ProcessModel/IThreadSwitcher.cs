using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LkeServices.ProcessModel
{
    public interface IThreadSwitcher
    {
        void SwitchThread(Func<Task> taskProc);
    }
}
