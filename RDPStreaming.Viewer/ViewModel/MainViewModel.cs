using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Protos;
using RDPStreaming.Viewer.Services;
using RDPStreaming.Viewer.View;
using RDPStreaming.Viewer.ViewModel.ICommands;

namespace RDPStreaming.Viewer.ViewModel
{
    public class MainViewModel : BaseViewModel, IDisposable
    {
        private ICommand _loginCommand;
        private DutyManagerService _dutyManagerService;
        private bool _loggedIn;
        private ActiveConnectionListViewModel _activeConnectionsViewModel;
        private ICommand _deactivateConnectionCommand;

        public ICommand DeactivateConnectionCommand
        {
            get
            {
                if (_deactivateConnectionCommand == null)
                {
                    _deactivateConnectionCommand = new RelayCommand(p => DeactivateConnection((ViewerViewModel)p), p => true);
                }
                return _deactivateConnectionCommand;
            }
        }

        public ObservableCollection<ViewerViewModel> ViewerViewModels { get; set; }

        public ActiveConnectionListViewModel ActiveConnectionsViewModel
        {
            get { return _activeConnectionsViewModel; }
            set { _activeConnectionsViewModel = value; OnPropertyChanged(nameof(ActiveConnectionsViewModel)); }
        }

        public bool LoggedIn
        {
            get { return _loggedIn; }
            set
            {
                _loggedIn = value;
                OnPropertyChanged(nameof(LoggedIn));
                if (_loggedIn)
                {
                    LoginSuccessful();
                }
                else
                {
                    LoginFailed();
                }
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(p => Login(), p => true);
                }
                return _loginCommand;
            }
        }

        public MainViewModel()
        {
            ViewerViewModels = new ObservableCollection<ViewerViewModel>();
            string server = ConfigurationManager.AppSettings.Get("Server");
            _dutyManagerService = new DutyManagerService(server);
        }

        private void Login()
        {
            var loginView = new LoginDialogView();
            var loginViewModel = new LoginDialogViewModel();
            loginView.DataContext = loginViewModel;
            loginView.ShowDialog();

            if (loginView.DialogResult.HasValue && !loginView.DialogResult.Value
                || !loginView.DialogResult.HasValue)
                return;

            LoggedIn = _dutyManagerService.Login(loginViewModel.Username, loginViewModel.Password);
            if (!LoggedIn)
            {
                MessageBox.Show("Login fehlgeschlagen");
                return;
            }

            MessageBox.Show("Login erfolgreich");
        }

        public void Dispose()
        {
            _dutyManagerService.Dispose();
        }

        private void LoginFailed()
        {
            ViewerViewModels.Clear();
            ActiveConnectionsViewModel?.Dispose();
            ActiveConnectionsViewModel = null;
        }

        private void LoginSuccessful()
        {
            ViewerViewModels.Clear();
            ActiveConnectionsViewModel?.Dispose();
            ActivateModuleActiveConnections();
            ActivateModuleActiveViewer();
        }

        private void ActivateModuleActiveConnections()
        {
            ActiveConnectionsViewModel = new ActiveConnectionListViewModel(_dutyManagerService, SelectConnectionCallback, StreamerDisconnectedCallback);
        }

        private void ActivateModuleActiveViewer()
        {
            //TODO
        }

        private void SelectConnectionCallback(StreamerClient client)
        {
            if (ViewerViewModels.Any(vm => vm.StreamerClient == client))
                return;

            var vm = new ViewerViewModel(client, _dutyManagerService);
            ViewerViewModels.Add(vm);
            vm.StartRendering();
        }

        private void StreamerDisconnectedCallback(string userKey)
        {
            if (!ViewerViewModels.Any(vm => vm.StreamerClient.Id == userKey))
                return;

            var foundViewModel = ViewerViewModels.FirstOrDefault(vm => vm.StreamerClient.Id == userKey);
            App.Current.Dispatcher.Invoke(() =>
            {
                DeactivateConnection(foundViewModel);
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void DeactivateConnection(ViewerViewModel viewerViewModel)
        {
            if (ViewerViewModels.Contains(viewerViewModel))
            {
                ViewerViewModels.Remove(viewerViewModel);
            }

            viewerViewModel?.Dispose();
        }
    }
}
