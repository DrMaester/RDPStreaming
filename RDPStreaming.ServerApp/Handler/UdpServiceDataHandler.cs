using RDPStreaming.Model;
using RDPStreaming.ServerApp.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RDPStreaming.ServerApp.Handler
{
    public class UdpServiceDataHandler : IDisposable
    {
        private ConcurrentBag<UdpClientData> _clients;
        private Socket _socket;

        public UdpServiceDataHandler(ConcurrentBag<UdpClientData> clients, Socket socket)
        {
            _socket = socket;
            _clients = clients;
        }

        public void Dispose()
        {
            _socket = default;
            _clients = default;
        }

        public void ConnectAsViewer(EndPoint endPoint, Paket paket)
        {
            var senderId = GetIdFromEndPoint(((IPEndPoint)endPoint));
            var client = _clients.FirstOrDefault(c => c.Id.ToString() == senderId);
            if (client != null)
            {
                client.JobId = Encoding.UTF8.GetString(paket.Data);
                client.IsMaster = true;
            }
            Console.WriteLine($"viewer {senderId} connected");
        }

        public void ConnectAsStreamer(EndPoint endPoint)
        {
            var senderId = GetIdFromEndPoint(((IPEndPoint)endPoint));
            Console.WriteLine($"streamer {senderId} connected");
        }

        public void ScreenCapture(EndPoint endPoint, Paket paket, byte[] buffer)
        {
            var control = _clients.FirstOrDefault(c => c.JobId == paket.JobId.ToString() && c.IsMaster && c.Connected);
            if (control != null)
                _socket.SendTo(buffer, control.IPEndPoint);
        }

        private string GetIdFromEndPoint(IPEndPoint iPEndPoint)
        {
            return _clients.FirstOrDefault(client => client.IPEndPoint.Address.ToString() == iPEndPoint.Address.ToString() &&
                    client.IPEndPoint.Port == iPEndPoint.Port).Id.ToString();
        }
    }
}
