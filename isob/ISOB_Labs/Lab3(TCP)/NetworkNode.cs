using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab3
{
    internal class NetworkNode
    {
        private const int _port = 1006;

        private static IPAddress _localIPAddress = IPAddress.Parse("127.0.0.1");

        private static IPEndPoint _localEndPoint = new IPEndPoint(_localIPAddress, _port);

        public static IPEndPoint ServerIP { get => _localEndPoint; }

        private readonly Dictionary<int, Socket> _clients = [];

        public async Task Listen(CancellationToken token)
        {
            var listenerSocket = ConfigureSocket();
            try
            {
                await SocketListening(listenerSocket, token);
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                listenerSocket.Close();
                listenerSocket.Dispose();
            }
        }

        private Socket ConfigureSocket()
        {
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenerSocket.ReceiveTimeout = 1000;

            listenerSocket.Bind(_localEndPoint);
            listenerSocket.Listen(10);

            return listenerSocket;
        }

        private async Task SocketListening(Socket listenerSocket, CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                Socket clientSocket = await listenerSocket.AcceptAsync(token);
                Console.WriteLine($"Client's socket has connected: {clientSocket.RemoteEndPoint}");

                _clients.Add((clientSocket.RemoteEndPoint as IPEndPoint).Port, clientSocket);

                new Thread(() => ProcessClientConnection((clientSocket.RemoteEndPoint as IPEndPoint).Port, token)).Start();
            }
        }


        private void ProcessClientConnection(object clientIdentifier, CancellationToken token)
        {
            int clientPort = (int)clientIdentifier;
            try
            {
                uint ackNumber, sequenceNumber = 0;
                ushort windowSize;

                NetworkSegment initialSegment = ReadSegment(clientPort, token).Result;

                if (!initialSegment.SYNFlag || initialSegment.Data.Length > 0)
                    throw new Exception($"Invalid initial connection segment from client {clientPort}: {initialSegment}");

                ackNumber = initialSegment.SequenceNumber + 1;
                windowSize = initialSegment.WindowSize;

                NetworkSegment synAckSegment = NetworkSegment.CreateEmptySegment(windowSize, _port, clientPort, sequenceNumber, ackNumber, syn: true, ack: true);
                SendSegment(clientPort, synAckSegment);

                NetworkSegment handshakeConfirmationSegment = ReadSegment(clientPort, token).Result;

                if (!handshakeConfirmationSegment.ACKFlag || handshakeConfirmationSegment.Data.Length > 0)
                    throw new Exception("Invalid connection confirmation segment from client.");

                Console.WriteLine("TCP handshake occured successfully\nMessage transmission started.\n");

                StringBuilder messageBuilder = new StringBuilder();
                for (int i = 0; i < 20; i++)
                {
                    messageBuilder.Append(i).Append(", ");
                }

                byte[] messageData = messageBuilder.ToString().ToUtf32Bytes();
                List<NetworkSegment> messageSegments = NetworkSegment.GetSegments(messageData, windowSize, _port, clientPort, sequenceNumber, ackNumber).ToList();

                foreach (NetworkSegment segmentToSend in messageSegments)
                {
                    Thread.Sleep(10);
                    SendSegment(clientPort, segmentToSend);

                    NetworkSegment acknowledgmentSegment = ReadSegment(clientPort, token).Result;

                    if (!acknowledgmentSegment.ACKFlag || acknowledgmentSegment.Data.Length > 0)
                        throw new Exception("Incorrect acknowledgment segment received from client.");

                    if (acknowledgmentSegment.RSTFlag)
                    {
                        Console.WriteLine($"Emergency connection termination with client {acknowledgmentSegment.SourcePort}.\nReceived segment: {acknowledgmentSegment}");
                        _clients[acknowledgmentSegment.SourcePort].Close();
                        return;
                    }

                    if (segmentToSend.SequenceNumber + windowSize != acknowledgmentSegment.AcknowledgmentNumber)
                        throw new Exception("Client acknowledged an incorrect set of data.");
                }

                SendSegment(clientPort, NetworkSegment.CreateEmptySegment(windowSize, _port, clientPort, sequenceNumber, ackNumber, fin: true));
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing client {clientPort}: {ex.Message}");
            }
            finally
            {
                if (_clients[clientPort].Connected)
                    _clients[clientPort].Close();
            }
        }
        private void SendSegment(int port, NetworkSegment segment)
        {
            byte[] responseBuffer = segment.ToJsonBytes();
            _clients[port].Send(responseBuffer);
        }

        public async Task<NetworkSegment> ReadSegment(int port, CancellationToken token)
        {
            byte[] messageBuffer = new byte[4096];
            List<byte> res = new();

            int bytesRead = await _clients[port].ReceiveAsync(messageBuffer, token);
            res.AddRange(messageBuffer[0..bytesRead]);

            return res.ToArray().DeserializeTcpSegment();
        }
    }
}
