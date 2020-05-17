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
        public Action<Job> StartStreamCallback { get; set; }
        public Action<Job> StopStreamCallback { get; set; }
        public Action<Job> CloseApplicationCallback { get; set; }

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

            StartStreamCallback = default;
            StopStreamCallback = default;
            CloseApplicationCallback = default;
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

        public async void StartListeningForJobsAsync()
        {
            var asyncStream = _client.StartJobObserver(new StringValue { Value = _userKey });
            while (await asyncStream.ResponseStream.MoveNext())
            {
                var currentJobRequest = asyncStream.ResponseStream.Current;
                switch (currentJobRequest.JobType)
                {
                    case Job.Types.JobType.None:
                        break;
                    case Job.Types.JobType.CloseApplication:
                        CloseApplicationCallback?.Invoke(currentJobRequest);
                        break;
                    case Job.Types.JobType.StartStreaming:
                        StartStreamCallback?.Invoke(currentJobRequest);
                        break;
                    case Job.Types.JobType.StopStreaming:
                        StopStreamCallback?.Invoke(currentJobRequest);
                        break;
                }
            }
        }

        private void StreamerHoldingLine()
        {
            _holdingLineRequest = _client.StreamerHoldingLine(new StringValue { Value = _userKey });
        }
    }
}
