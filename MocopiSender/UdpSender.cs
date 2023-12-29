#nullable enable

using System.Net;
using System.Net.Sockets;

namespace MocopiSender
{
    class UdpSender
    {
        private Socket udpSocket;
        private const int REMOTE_PORT = 12351;

        internal UdpSender(string remoteAddress)
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Connect(new IPEndPoint(IPAddress.Parse(remoteAddress), REMOTE_PORT));
        }

        internal void Close()
        {
            udpSocket.Close();
        }

        internal void SendData(byte[] data)
        {
            udpSocket.Send(data, data.Length, SocketFlags.None);
        }
    }
}