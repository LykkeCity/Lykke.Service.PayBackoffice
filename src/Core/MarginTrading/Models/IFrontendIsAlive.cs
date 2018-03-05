using System;

namespace Core.MarginTrading.Models
{
    public interface IFrontendIsAlive
    {
        string DemoVersion { get; }
        string LiveVersion { get; }
        int WampOpened { get; }
        string Version { get; }
        string Env { get; }
        DateTime ServerTime { get; }
    }
}
