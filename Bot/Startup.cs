using BotApp;
using MamaBot.GlobalShared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
    public class Startup
    {
        public BotConfig BotConfig { get; }

        public Startup(IConfiguration configuration)
        {
            BotConfig = new BotConfig(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<AppService>();
            services.AddSingleton(BotConfig);
            services.AddSingleton<BotIstance>();
        }
    }

    public class AppService : IHostedService
    {
        private readonly BotIstance _instance;

        public AppService(BotIstance instance, ILoggerFactory loggerFactory)
        {
            Vars.Logger = loggerFactory.CreateLogger(typeof(Vars));
            _instance = instance;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _instance.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _instance.StopAsync(cancellationToken);
        }
    }
}
