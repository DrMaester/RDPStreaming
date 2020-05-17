using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Protos;
using RDPStreaming.ServerApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RDPStreaming.ServerApp.ProtoForwarder
{
    public class DutyManagerForwarder : Protos.DutyManager.DutyManagerBase
    {
        private readonly LoginService _loginService;

        public DutyManagerForwarder(LoginService loginService)
        {
            this._loginService = loginService;
        }

        public override async Task NewStreamerStream(StringValue request, IServerStreamWriter<StreamerClient> responseStream, ServerCallContext context)
        {
            var cancellationToken = new CancellationTokenSource();
            _loginService.AddNewStreamerObserver(request.Value, responseStream, cancellationToken);
            Task t = Task.Run(() =>
            {
                do
                {
                    Task.Delay(1000).Wait();
                } while (!cancellationToken.IsCancellationRequested && !context.CancellationToken.IsCancellationRequested);
            });
            await t;
        }

        public override async Task RemoveStreamerStream(StringValue request, IServerStreamWriter<StreamerClient> responseStream, ServerCallContext context)
        {
            var cancellationToken = new CancellationTokenSource();
            _loginService.AddRemoveStreamerObserver(request.Value, responseStream, cancellationToken);
            Task t =  Task.Run(() =>
            {
                do
                {
                    Task.Delay(1000).Wait();
                } while (!cancellationToken.IsCancellationRequested && !context.CancellationToken.IsCancellationRequested);
            });
            await t;
        }

        public override async Task<StringValue> LoginAsStreamer(StringValue request, ServerCallContext context)
        {
            var userKey = new StringValue { Value = await _loginService.LoginAsStreamerAsync(request.Value) };
            return userKey;
        }

        public override async Task<StringValue> Login(Credentials request, ServerCallContext context)
        {
            var stringValue = new StringValue { Value = await _loginService.LoginAsync(request) };
            return stringValue;
        }

        public override async Task StreamerHoldingLine(StringValue request, IServerStreamWriter<Empty> responseStream, ServerCallContext context)
        {
            var userkey = request.Value;
            try
            {
                do
                {
                    await Task.Delay(5000);
                    await responseStream.WriteAsync(new Empty());
                } while (!context.CancellationToken.IsCancellationRequested);
            }
            catch (InvalidOperationException) // Streamer disconnects -> writing failure
            {
                _loginService.StreamerDisconnected(userkey);
            }
        }
    }
}
