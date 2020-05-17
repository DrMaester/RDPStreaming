using log4net;
using log4net.Config;
using RDPStreaming.Viewer.Logging;
using RDPStreaming.Viewer.View;
using RDPStreaming.Viewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace RDPStreaming.Viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly string _logConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");

        protected override void OnStartup(StartupEventArgs e)
        {
            InitLogger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Logger.Get().Info("Application started");
            var view = new MainView();
            var vm = new MainViewModel();
            view.DataContext = vm;
            MainWindow = view;
            MainWindow.Show();
            view.Closed += MainView_Closed;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Get().Fatal(((Exception)e.ExceptionObject).Message, e.ExceptionObject as Exception);
            Environment.Exit(1);
        }

        private void MainView_Closed(object sender, EventArgs e)
        {
            var view = sender as MainView;
            var vm = view.DataContext as MainViewModel;
            vm.Dispose();
        }

        private void InitLogger()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(_logConfigFilePath));
        }
    }
}
