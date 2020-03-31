using Microsoft.Extensions.Configuration;

namespace Bot
{
    public class BotConfig
    {
        private readonly IConfiguration _config;

        public BotConfig(IConfiguration config)
        {
            _config = config;
        }

        public string Pair => _config.GetValue<string>("Pair");
        public string ApiKey => _config.GetValue<string>("ApiKey");
        public string ApiSecret => _config.GetValue<string>("ApiSecret");
    }
}
