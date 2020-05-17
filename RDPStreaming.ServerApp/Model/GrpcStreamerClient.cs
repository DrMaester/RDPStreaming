using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPStreaming.ServerApp.Model
{
    public class GrpcStreamerClient
    {
        public string Computername { get; set; }
        public string Id { get; set; }

        public GrpcStreamerClient(string computername)
        {
            Computername = computername;
            Id = Guid.NewGuid().ToString();
        }
    }
}
