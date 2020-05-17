using Grpc.Core;
using Protos;
using RDPStreaming.ServerApp.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RDPStreaming.ServerApp.Services
{
    public class LoginService : IDisposable
    {
        private readonly object _newStreamerObserverLock = new object();
        private readonly object _removeStreamerObserverLock = new object();

        private List<Credentials> _masterCredentials;
        private ConcurrentBag<string> _authToken;
        private ConcurrentBag<StreamerClient> _streamer;
        private ConcurrentBag<string> _viewer;
        private Dictionary<string, IServerStreamWriter<StreamerClient>> _newStreamerObserver;
        private Dictionary<string, IServerStreamWriter<StreamerClient>> _removeStreamerObserver;

        public LoginService()
        {
            _masterCredentials = new List<Credentials>
            {
                new Credentials
                {
                    Username = "Daniel",
                    Password = "qwe321"
                }
            };
            _authToken = new ConcurrentBag<string>();
            _streamer = new ConcurrentBag<StreamerClient>();
            _viewer = new ConcurrentBag<string>();
            _newStreamerObserver = new Dictionary<string, IServerStreamWriter<StreamerClient>>();
            _removeStreamerObserver = new Dictionary<string, IServerStreamWriter<StreamerClient>>();
        }

        public void Dispose()
        {
            // TODO
        }

        public Task<string> LoginAsStreamerAsync(string computerName)
        {
            var streamerClient = new StreamerClient
            {
                ComputerName = computerName,
                Id = Guid.NewGuid().ToString()
            };
            _streamer.Add(streamerClient);
            InformObserverNewStreamer(streamerClient);
            return Task.FromResult(streamerClient.Id);
        }

        public Task<string> LoginAsync(Credentials credentials)
        {
            if (AreCredentialsValid(credentials))
            {
                return Task.FromResult(CreateNewAuthToken());
            }

            return Task.FromResult(string.Empty);
        }


        public bool IsAuthTokenValid(string authToken)
        {
            return _authToken.Any(s => s == authToken);
        }

        public void AddNewStreamerObserver(string authKey, IServerStreamWriter<StreamerClient> observer, CancellationTokenSource token)
        {
            if (!IsAuthTokenValid(authKey))
            {
                token.Cancel();
                return;
            }
            lock (_newStreamerObserverLock)
            {
                _newStreamerObserver.Add(authKey, observer);
                foreach (var streamer in _streamer.ToArray())
                {
                    observer.WriteAsync(streamer);
                }
            }
        }

        public void AddRemoveStreamerObserver(string authKey, IServerStreamWriter<StreamerClient> observer, CancellationTokenSource token)
        {
            if (!IsAuthTokenValid(authKey))
            {
                token.Cancel();
                return;
            }
            lock (_removeStreamerObserverLock)
            {
                _removeStreamerObserver.Add(authKey, observer);
            }
        }

        public void StreamerDisconnected(string userkey)
        {
            var foundClient = _streamer.FirstOrDefault(s => s.Id == userkey);
            if (foundClient == null)
                return;

            InformObserverRemoveStreamer(foundClient);
        }

        private void InformObserverNewStreamer(StreamerClient client)
        {
            lock (_newStreamerObserverLock)
            {
                foreach (var observer in _newStreamerObserver)
                {
                    observer.Value.WriteAsync(client);
                }
            }
        }

        private void InformObserverRemoveStreamer(StreamerClient client)
        {
            lock (_removeStreamerObserverLock)
            {
                foreach (var observer in _removeStreamerObserver)
                {
                    observer.Value.WriteAsync(client);
                }
            }
        }

        private bool AreCredentialsValid(Credentials credentials)
        {
            return _masterCredentials.Any(c => c.Username.ToLower() == credentials.Username.ToLower() && c.Password == credentials.Password);
        }

        private string CreateNewAuthToken()
        {
            var authToken = Guid.NewGuid().ToString();
            _authToken.Add(authToken);
            return authToken;
        }
    }
}
