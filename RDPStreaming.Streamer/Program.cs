using RDPStreaming.Streamer.Logging;
using RDPStreaming.Streamer.Services;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RDPStreaming.Streamer
{
    class Program
    {
        private static DutyManagerService _dutyManagerService;

        static async Task Main(string[] args)
        {
            InitLogger();
            Logger.Get().Info("Application started");
            var server = ConfigurationManager.AppSettings.Get("Server");
            _dutyManagerService = new DutyManagerService(server);
            bool isLoggedIn = false;
            do
            {
                isLoggedIn = await _dutyManagerService.LoginAsync();
                await Task.Delay(TimeSpan.FromSeconds(1));
            } while (!isLoggedIn);

            Console.ReadKey();
        }

        private static void InitLogger()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            var logRepository = log4net.LogManager.GetRepository(Assembly.GetExecutingAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo(path));
        }
    }
}
