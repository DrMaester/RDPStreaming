using RDPStreaming.Model;
using RDPStreaming.Streamer.Logging;
using RDPStreaming.Streamer.Model;
using RDPStreaming.Streamer.Services;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RDPStreaming.Streamer
{
    class Program
    {
        private static DutyManagerService _dutyManagerService;
        private static UdpService _udpService;
        private static ScreenCaptureService _screenCaptureService;
        private static CancellationTokenSource _screenCaptureServiceToken;

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

            _dutyManagerService.CloseApplicationCallback = CloseApplication;
            _dutyManagerService.StartStreamCallback = StartStreaming;
            _dutyManagerService.StopStreamCallback = StopStreaming;
            _dutyManagerService.StartListeningForJobsAsync();

            InitUdpService();

            Console.ReadKey();
        }

        private static void InitLogger()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            var logRepository = log4net.LogManager.GetRepository(Assembly.GetExecutingAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo(path));
        }

        private static void InitUdpService()
        {
            var ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 7000;
            _udpService = new UdpService(ipAddress, port);
        }

        private static void CloseApplication(Protos.Job job)
        {
            Console.WriteLine("closing now..");
            Environment.Exit(0);
        }

        private static void StartStreaming(Protos.Job job)
        {
            _screenCaptureServiceToken = new CancellationTokenSource();
            _screenCaptureService = new ScreenCaptureService(NewBitmap);
            _screenCaptureService.StartCaptureAsync(_screenCaptureServiceToken.Token);
            Console.WriteLine("new job request: start streaming");

        }

        private static void NewBitmap(BitmapBufferedData[] croppedBitmaps)
        {

        }

        private static void StopStreaming(Protos.Job job)
        {
            Console.WriteLine("new job request: stop streaming");
        }
    }
}
