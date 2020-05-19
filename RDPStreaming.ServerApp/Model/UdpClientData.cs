using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RDPStreaming.ServerApp.Model
{
    public class UdpClientData
    {
        public Guid Id { get; set; }
        public string JobId { get; set; }
        public IPEndPoint IPEndPoint { get; set; }
        public bool Connected { get; set; }
        public bool IsMaster { get; set; }

        public UdpClientData(IPEndPoint iPEndPoint)
        {
            Id = Guid.NewGuid();
            IPEndPoint = iPEndPoint;
            Connected = true;
        }
    }
}
