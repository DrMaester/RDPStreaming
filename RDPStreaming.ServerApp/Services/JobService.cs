using Grpc.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Protos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPStreaming.ServerApp.Services
{
    public class JobService : IDisposable
    {
        private ConcurrentDictionary<string, IServerStreamWriter<Job>> _observerStreams;

        public JobService()
        {
            _observerStreams = new ConcurrentDictionary<string, IServerStreamWriter<Job>>();
        }

        public void Dispose()
        {
            _observerStreams?.Clear();
            _observerStreams = default;
        }

        public void AddJobObserver(string streamerId, IServerStreamWriter<Job> observerStream)
        {
            _observerStreams.TryAdd(streamerId, observerStream);
        }

        public void NewJobRequest(AuthJobRequest authJobRequest)
        {
            if (!_observerStreams.Any(o => o.Key == authJobRequest.StreamerId))
                return;

            var foundObserver = _observerStreams.FirstOrDefault(o => o.Key == authJobRequest.StreamerId);
            foundObserver.Value.WriteAsync(authJobRequest.Job);
        }
    }
}
