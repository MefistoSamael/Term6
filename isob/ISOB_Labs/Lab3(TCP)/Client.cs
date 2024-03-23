using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab3
{
    internal class Client
    {
        private readonly IPEndPoint _ip = new(IPAddress.Parse("127.0.0.1"), Constants.ClientPort);

        public IPEndPoint ClientIP { get => _ip; }

        public Task ConnectToServer(IPEndPoint serverIP, CancellationTokenSource cancelTokenSource)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Bind(_ip);
            

            try
            {
                clientSocket.Connect(serverIP);


                SendSegment(clientSocket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, serverIP.Port, 0, 0, syn: true)); 
                
                NetworkSegment segment = ReadSegment(clientSocket);

                if (!segment.SYNFlag || !segment.ACKFlag)
                    throw new Exception("Incorrect initial segment from server");

                SendSegment(clientSocket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, serverIP.Port, 1, 1, ack: true));


                StringBuilder serverMessage = new();

                segment = ReadSegment(clientSocket);
                do
                {
                    serverMessage.Append(segment.Data.Utf32BytesToString());

                    Thread.Sleep(10);
                    SendSegment(clientSocket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, serverIP.Port, 1, segment.SequenceNumber + (uint)segment.Data.Length, ack: true));
                    segment = ReadSegment(clientSocket);
                }
                while (!segment.FINFlag);

                Console.WriteLine("\n\n\n" +
                                  "Message received from server: " + serverMessage +
                                  "\n\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error while working with the server: {ex.Message}");
            }
            finally
            {
                if (clientSocket.Connected)
                    clientSocket.Disconnect(false);

                clientSocket.Close();
            }

            cancelTokenSource.Cancel();
            return Task.CompletedTask;
        }

        private void SendSegment(Socket socket, NetworkSegment segment)
        {
            byte[] responseBuffer = segment.ToJsonBytes();
            socket.Send(responseBuffer);
        }

        public NetworkSegment ReadSegment(Socket socket)
        {
            byte[] messageBuffer = new byte[4096];
            List<byte> res = new();

            int bytesRead = socket.Receive(messageBuffer);
            res.AddRange(messageBuffer[0..bytesRead]);

            return res.ToArray().DeserializeTcpSegment();
        }
    }
}
