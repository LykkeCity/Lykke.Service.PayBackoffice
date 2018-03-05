using System;

namespace Core.ProcessModel
{
    public class DisposeAction : IDisposable
    {
        private Action _disposeAction;

        public DisposeAction(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            var disposeAction = _disposeAction;
            if (null == disposeAction)
                return;

            lock (disposeAction)
            {
                if (null == _disposeAction)
                    return;
                _disposeAction = null;
            }

            disposeAction();
        }
    }
}
