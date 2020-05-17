using Protos;
using RDPStreaming.Viewer.Services;
using RDPStreaming.Viewer.ViewModel.ICommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace RDPStreaming.Viewer.ViewModel
{
    public class ActiveConnectionListViewModel : BaseViewModel, IDisposable
    {
        private DutyManagerService _dutyManagerService;
        private readonly object _streamerClientsLock = new object();
        private Action<StreamerClient> _selectCallback;
        private Action<string> _streamerDisconnectedCallback;
        private ICommand _selectCommand;

        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand(p => _selectCallback?.Invoke((StreamerClient)p), p => true);
                }
                return _selectCommand;
            }
        }

        public ObservableCollection<StreamerClient> StreamerClients { get; set; }

        public ActiveConnectionListViewModel(DutyManagerService dutyManagerService, Action<StreamerClient> selectCallback, 
            Action<string> streamerDisconnectedCallback)
        {
            StreamerClients = new ObservableCollection<StreamerClient>();
            _selectCallback = selectCallback;
            _streamerDisconnectedCallback = streamerDisconnectedCallback;
            _dutyManagerService = dutyManagerService;
            _dutyManagerService.StartObserveForNewStreamer(NewStreamerCallback);
            _dutyManagerService.StartObserveForRemoveStreamer(RemoveStreamerCallback);
        }

        public void Dispose()
        {
            _dutyManagerService = null;
            StreamerClients?.Clear();
            _selectCallback = default;
            _streamerDisconnectedCallback = default;
        }

        private void NewStreamerCallback(StreamerClient client)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                lock (_streamerClientsLock)
                {
                    StreamerClients.Add(client);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void RemoveStreamerCallback(StreamerClient client)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                lock (_streamerClientsLock)
                {
                    var toRemove = StreamerClients.FirstOrDefault(c => c.Id == client.Id);
                    if (toRemove != null)
                    {
                        StreamerClients.Remove(toRemove);
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
            _streamerDisconnectedCallback?.Invoke(client.Id);
        }
    }
}
