using System;

namespace Core.MarginTrading.Models
{
    public class FrontendIsAlive : IFrontendIsAlive
    {
        public string DemoVersion { get; set; }
        public string LiveVersion { get; set; }
        public int WampOpened { get; set; }
        public string Version { get; set; }
        public string Env { get; set; }
        public DateTime ServerTime { get; set; }
    }
}
