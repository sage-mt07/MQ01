using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MQLib;


namespace WorkerService1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    var mqSettings = configuration.GetSection("MQConfigurations").Get<MQConfiguration>();

                    services.AddSingleton(new ConnectionManager(mqSettings));
                    services.AddSingleton<Messaging>();
                    services.AddHostedService<Worker>();
                    services.AddLogging(config => config.AddConsole());
                })
                .Build();
            host.Run();
        }
    }
}
