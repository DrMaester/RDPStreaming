using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RDPStreaming.Model;
using RDPStreaming.Provider;

namespace RDPStreaming.Streamer.Services
{
    public class UdpService : IDisposable
    {
        private int _port;
        private IPAddress _serverIp;
        private IPEndPoint _serverIPEndpoint;
        private UdpClient _client;
        private int _paketSize = PaketSizeProvider.GetMaxPaketSize();

        public UdpService(IPAddress iPAddress, int port)
        {
            _serverIp = iPAddress;
            _port = port;
            _serverIPEndpoint = new IPEndPoint(_serverIp, _port);
            _client = new UdpClient();
            _client.Client.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.AllowNatTraversal(true);
        }

        public void Connect()
        {
            Task.Run(() => Receive());
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public void TransmitScreenBufferPaket(byte[] buffer)
        {
            _client.Client.SendTo(buffer, _serverIPEndpoint);
        }

        private void Receive()
        {
            try
            {
                var connectPaket = new Paket(PaketType.Connect, null);
                var connectPaketBuffer = connectPaket.ToBytes();
                _client.Client.SendTo(connectPaketBuffer, _serverIPEndpoint);
                while (true)
                {
                    var paketBuffer = _client.Receive(ref _serverIPEndpoint);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("\n$$$ -> " + ex.Message + "\n");
            }
        }
    }
}

