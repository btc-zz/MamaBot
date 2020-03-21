using System.Threading.Tasks;
using MaMa.HFT;
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

            
            BotIstance test = new BotIstance("BTCUSDT", Vars.BinanceApiKey, Vars.BinanceApiSecret);
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
