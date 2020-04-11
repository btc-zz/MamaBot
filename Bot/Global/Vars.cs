using Bot.Channel;
using Bot.Services.Orderbook;
using MaMa.HFT.Console;
using Microsoft.Extensions.Logging;

namespace MamaBot.GlobalShared
{
    public static class Vars
    {
        public static bool ShowOrderErrors = false;
        public static bool IsRunning = true;
        public static bool IsShutingdown = false;

        //public static Perf ThreadManager = new Perf();
        public static ILogger Logger { get; set; }
        public static OrderQueue OrderChannel { get; set; } = new OrderQueue();
        public static TickQueue TickChannel { get; set; } = new TickQueue();
        public static CandleService Candleservice { get; set; } = new CandleService();
        public static OrderFlowStatistics FlowComputeService { get; set; } = new OrderFlowStatistics();
    }
}
