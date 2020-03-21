using System;
using System.Collections.Generic;
using System.Text;

namespace MamaBot.GlobalShared
{
    public static class Vars
    {
        public static string BinanceApiKey = Environment.GetEnvironmentVariable("ApiKey", EnvironmentVariableTarget.User);
        public static string BinanceApiSecret = Environment.GetEnvironmentVariable("ApiSecret", EnvironmentVariableTarget.User);
        public static bool ShowOrderErrors = false;
        public static bool IsRunning = true;
        public static Perf ThreadManager = new Perf();
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    }
}
