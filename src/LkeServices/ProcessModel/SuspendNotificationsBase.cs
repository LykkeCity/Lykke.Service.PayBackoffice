using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.ProcessModel
{
    public class SuspendNotificationsBase : ISuspendNotifications
    {
        private int _suspendLevel = 0;
        // Acts as sync for _suspendLevel
        private readonly ConcurrentQueue<Action> _typeNotifiers = new ConcurrentQueue<Action>();
        private readonly Dictionary<Type, List<Action<object, object>>> _notifyActions = new Dictionary<Type, List<Action<object, object>>>();

        #region Suspention and resumption
        public IDisposable SuspendNotifications()
        {
            lock (_typeNotifiers)
            {
                _suspendLevel++;
                return new DisposeAction(ResumeNotifications);
            }
        }

        private void ResumeNotifications()
        {
            lock (_typeNotifiers)
            {
                if (0 >= _suspendLevel)
                    throw new InvalidOperationException("Inconsistent notifications resumption");
                _suspendLevel--;
            }
            SendNotifications();
        }
        #endregion

        #region Notifications
        protected void Notify<TEventArgs>(object sender, TEventArgs eventArgs)
        {
            List<Action<object, object>> notifiers;
            lock (notifiers = GetNotifiers(typeof(TEventArgs), true))
            {
                foreach(var notifier in notifiers)
                    _typeNotifiers.Enqueue(() => { notifier(sender, eventArgs); });
            }
            SendNotifications();
        }

        private void SendNotifications()
        {
            Action notificationAction;
            while (0 >= _suspendLevel && _typeNotifiers.TryDequeue(out notificationAction))
                Task.Run(notificationAction);
        }
        #endregion

        #region Notifiers
        protected void RegisterEventNotifier<TEventArgs>(Action<object, TEventArgs> notifier)
        {
            List<Action<object, object>> typeNotifiers;
            lock (typeNotifiers = GetNotifiers(typeof(TEventArgs), true))
                typeNotifiers.Add((sender, eventArgs) => notifier(sender, (TEventArgs)eventArgs));
        }

        private List<Action<object, object>> GetNotifiers(Type eventArgsType, bool createIfNotExists = false)
        {
            lock (_notifyActions)
            {
                if (_notifyActions.ContainsKey(eventArgsType))
                    return _notifyActions[eventArgsType];

                if (!createIfNotExists)
                    return null;

                var typeNotifiers = new List<Action<object, object>>();
                _notifyActions.Add(eventArgsType, typeNotifiers);
                return typeNotifiers;
            }
        }
        #endregion
    }
}
