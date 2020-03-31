using Microsoft.Extensions.Logging;

namespace MamaBot.GlobalShared
{
    public static class Vars
    {
        public static bool ShowOrderErrors = false;
        public static bool IsRunning = true;
        public static bool IsShutingdown = false;

        public static Perf ThreadManager = new Perf();
        public static ILogger Logger { get; set; }
    }
}
