﻿using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using Google.Protobuf.WellKnownTypes;
using System.Windows.Input;
using RDPStreaming.Viewer.Logging;
using Protos;
using System.Threading.Tasks;
using System.Threading;

namespace RDPStreaming.Viewer.Services
{
    public class DutyManagerService : IDisposable
    {
        private string _address;
        private GrpcChannel _channel;
        private Protos.DutyManager.DutyManagerClient _client;
        private string _authKey;

        public DutyManagerService(string address)
        {
            _address = address;
            _channel = GrpcChannel.ForAddress(_address);
            _client = new Protos.DutyManager.DutyManagerClient(_channel);
        }

        public async void Dispose()
        {
            _client = null;
            await _channel.ShutdownAsync();
            _channel = null;
        }

        public async void StartObserveForNewStreamer(Action<StreamerClient> newStreamerClientCallback)
        {
            await Task.Run(async() =>
            {
                var stream = _client.NewStreamerStream(new StringValue { Value = _authKey });
                while (await stream.ResponseStream.MoveNext(CancellationToken.None))
                {
                    newStreamerClientCallback?.Invoke(stream.ResponseStream.Current);
                }
            });
        }

        public async void StartObserveForRemoveStreamer(Action<StreamerClient> removeStreamerClientCallback)
        {
            await Task.Run(async () =>
            {
                var stream = _client.RemoveStreamerStream(new StringValue { Value = _authKey });
                while (await stream.ResponseStream.MoveNext(CancellationToken.None))
                {
                    removeStreamerClientCallback?.Invoke(stream.ResponseStream.Current);
                }
            });
        }

        public bool Login(string username, string password)
        {
            var credentials = new Protos.Credentials()
            {
                Username = username,
                Password = password
            };
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                StringValue key = _client.Login(credentials);
                if (string.IsNullOrEmpty(key.Value))
                    return false;

                _authKey = key.Value;
                return true;
            }
            catch (Grpc.Core.RpcException ex)
            {
                Logger.Get().Error(ex.Message);
                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

        }
    }
}