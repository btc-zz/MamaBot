using System.IO;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            host.Start();
            await host.WaitForShutdownAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
#if DEBUG
            var env = "Development";
#else
            var env = "Production";
#endif

            return new HostBuilder()
                .UseEnvironment(env)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                    configHost.AddEnvironmentVariables(prefix: "ASPNETCORE_");
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostingContext, configApp) => Configure(configApp, env, args))
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddFile(hostingContext.Configuration.GetSection("Logging"));
                })
                .ConfigureServices((hostContext, services)
                    => new Startup(hostContext.Configuration).ConfigureServices(services));
        }

        private static IConfigurationBuilder Configure(IConfigurationBuilder builder, string env, string[] args)
        {
            return builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Bot.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"Bot.{env}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
        }
    }
}
