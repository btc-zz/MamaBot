﻿using System.Threading.Tasks;
using MaMa.HFT.Console.GlobalShared;
using MamaBot;
using MamaBot.GlobalShared;

namespace MaMa.HFT.Console
{
    public class Program
    {


        public static async Task Main(string[] args)
        {
            // set bot settings
            const string token = "BTCUSDT";

            
            Instance test = new Instance("BTCUSDT", Vars.BinanceApiKey, Vars.BinanceApiSecret);

            //CandleService LoadData = new CandleService("BTCUSDT");



            try
            {
                //await bot.RunAsync();

                System.Console.WriteLine("Press Enter to stop bot...");
                System.Console.ReadLine();
            }
            finally
            {
                //bot.Stop();
            }
           
            System.Console.WriteLine("Press Enter to exit...");
            System.Console.ReadLine();
        }   
    }
}
