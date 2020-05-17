using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Protos;
using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using Grpc.Core;
using RDPStreaming.Streamer.Logging;
using System.Threading;

namespace RDPStreaming.Streamer.Services
{
    public class DutyManagerService : IDisposable
    {
        private string _userKey;
        private GrpcChannel _channel;
        private DutyManager.DutyManagerClient _client;
        private AsyncServerStreamingCall<Empty> _holdingLineRequest;

        public DutyManagerService(string address)
        {
            _channel = GrpcChannel.ForAddress(address);
            _client = new DutyManager.DutyManagerClient(_channel);
        }

        public void Dispose()
        {
            _holdingLineRequest?.Dispose();
            _channel?.ShutdownAsync().Wait(TimeSpan.FromSeconds(5));
            _channel?.Dispose();
        }

        public async Task<bool> LoginAsync()
        {
            try
            {
                var mashineName = Environment.MachineName;
                var userKey = await _client.LoginAsStreamerAsync(new StringValue { Value = mashineName });
                _userKey = userKey.Value;
                StreamerHoldingLine();
                Logger.Get().Info($"Login success.. userKey: {_userKey}");
            }
            catch (RpcException ex)
            {
                Logger.Get().Error(ex.Message);
                return false;
            }

            return true;
        }

        private void StreamerHoldingLine()
        {
            _holdingLineRequest = _client.StreamerHoldingLine(new StringValue { Value = _userKey });
        }
    }
}
