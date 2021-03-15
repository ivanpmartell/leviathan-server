using System;
using System.Net.Sockets;
using System.Net;

namespace leviathan_server
{
    class Program
    {
        private const int listenPort = 16700;

        private void StartListener()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port intTemp.
                Int32 intPort = listenPort;
                IPAddress localAddr = IPAddress.Parse("0.0.0.0");
                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, intPort);
                // Start listening for client requests.
                server.Start();
                // Buffer for reading data
                Byte[] bytes = new Byte[4096];
                 
                String data = null;
                // Enter the listening loop.
                while (true)
                {
                    Console.WriteLine("Listening on {0}... ", intPort);
                    TcpClient client = server.AcceptTcpClient(); // server.AcceptSocket() also good
                    Console.WriteLine("> Connected to {0}", client.Client.RemoteEndPoint);
                    data = null;
                    NetworkStream stream = client.GetStream();

                    int intLoop;
                    while ((intLoop = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, intLoop);
                        Console.WriteLine("Received: {0} from {1}", data, client.Client.RemoteEndPoint);
                        // Process the data sent by the client.
                        data = data.ToUpper();
                        if (data == "Q") return;
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        // Send back a response.                        
                        stream.Write(msg, 0, msg.Length);                        
                        Console.WriteLine("Sent: {0} to {1}", data, client.Client.RemoteEndPoint);
                    }
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.StartListener();
        }

    }
}