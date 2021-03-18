using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using PTech;

namespace leviathan_server
{
    class Server3
    {
        PTech.RPC m_serverRpc;
        public Server3(string ip, int port)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            socket.NoDelay = true;
            socket.Blocking = false;
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            socket.Bind(localEndPoint);
            socket.Listen(100);
            Console.WriteLine("Started server on port " + port);
            Socket socket3 = null;
            while (socket3 == null)
            {
                try
                {
                    socket3 = socket.Accept();
                    socket3.Blocking = false;
                }
                catch (SocketException)
                {
                }
            }
            PacketSocket socket4 = new PacketSocket(socket3);
            m_serverRpc = new PTech.RPC(socket4);
        }
    }
}