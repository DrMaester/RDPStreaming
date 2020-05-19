using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RDPStreaming.Model;
using RDPStreaming.Provider;
using RDPStreaming.ServerApp.Handler;
using RDPStreaming.ServerApp.Model;

namespace RDPStreaming.ServerApp.Services
{
    public class UdpService : IDisposable
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private EndPoint _endPoint;
        private ConcurrentBag<UdpClientData> _clients;
        private UdpServiceDataHandler _dataHandler;
        private int _buffersize = PaketSizeProvider.GetMaxPaketSize();
        private int _port;

        const uint IOC_IN = 0x80000000;
        const uint IOC_VENDOR = 0x18000000;
        uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

        public UdpService(int port)
        {
            _port = port;
            _clients = new ConcurrentBag<UdpClientData>();
            _endPoint = new IPEndPoint(IPAddress.Any, port);
            _socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            _socket.Bind(_endPoint);
            _dataHandler = new UdpServiceDataHandler(_clients, _socket);
        }

        public void Dispose()
        {
            _clients?.Clear();
            _clients = default;

            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Dispose();
            _socket = default;

            _endPoint = default;

            _dataHandler?.Dispose();
            _dataHandler = default;
        }

        public void Start()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        byte[] buffer = new byte[_buffersize];
                        _endPoint = new IPEndPoint(IPAddress.Any, _port);

                        var readed = _socket.ReceiveFrom(buffer, ref _endPoint);
                        buffer = buffer.Take(readed).ToArray();

                        var paket = new Paket(buffer);
                        if (paket.PaketType == PaketType.Connect &&
                        !_clients.Any(client => client.IPEndPoint.Address.ToString() == ((IPEndPoint)_endPoint).Address.ToString() &&
                            client.IPEndPoint.Port == ((IPEndPoint)_endPoint).Port))
                        {
                            _clients.Add(new UdpClientData(((IPEndPoint)_endPoint)));
                        }
                        DataHandler(_endPoint, paket, buffer);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

            });
            Console.WriteLine($"UdpService running on {_endPoint}");
        }

        private void DataHandler(EndPoint endPoint, Paket paket, byte[] buffer)
        {
            switch (paket.PaketType)
            {
                case PaketType.Connect:
                    _dataHandler.ConnectAsStreamer(endPoint);
                    break;
                case PaketType.ConnectAsControl:
                    _dataHandler.ConnectAsViewer(endPoint, paket);
                    break;
                case PaketType.ScreenCapture:
                    break;
                default:
                    break;
            }
        }
    }
}
