using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab3
{
    internal class NetworkNode
    {
        private const int _port = 1000;

        private static IPAddress _localIPAddress = IPAddress.Parse("127.0.0.1");

        private static IPEndPoint _localEndPoint = new IPEndPoint(_localIPAddress, _port);

        public static IPEndPoint ServerIP { get => _localEndPoint; }

        private readonly Dictionary<int, Socket> _clients = [];

        public async Task Listen(CancellationToken token)
        {
            try
            {
                var listenerSocket = ConfigureSocket();

                await SocketListening(listenerSocket, token);
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private Socket ConfigureSocket()
        {
            // Создаем объект Socket для прослушивания указанного IP адреса и порта
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Время ожиданий ответа равно 1 секунду
            listenerSocket.ReceiveTimeout = 1000;

            // Привязываем сокет к конечной точке
            listenerSocket.Bind(_localEndPoint);
            // Начинаем прослушивание с максимальной длиной очереди подключений в 10
            listenerSocket.Listen(10);
            Console.WriteLine($"Server has started at {_localEndPoint}");

            return listenerSocket;
        }

        private async Task SocketListening(Socket listenerSocket, CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                Socket clientSocket = await listenerSocket.AcceptAsync(token);
                Console.WriteLine($"Client has connected: {clientSocket.RemoteEndPoint}");

                _clients.Add((clientSocket.RemoteEndPoint as IPEndPoint).Port, clientSocket);

                Thread clientThread = new Thread(new ParameterizedThreadStart(ProcessClientConnection));
                clientThread.Start((clientSocket.RemoteEndPoint as IPEndPoint).Port);
            }
        }


        private void ProcessClientConnection(object clientIdentifier)
        {
            int clientPort = (int)clientIdentifier;
            try
            {
                // Client connection variables
                uint ackNumber, sequenceNumber = 0;
                ushort windowSize;

                // Client connection handshake
                NetworkSegment initialPacket = ReadPacket(clientPort);

                if (!initialPacket.SYNFlag || initialPacket.Data.Length > 0)
                    throw new Exception($"Invalid initial connection packet from client {clientPort}: {initialPacket}");

                ackNumber = initialPacket.SequenceNumber + 1;
                windowSize = initialPacket.WindowSize;

                NetworkSegment synAckPacket = NetworkSegment.CreateEmptySegment(windowSize, _port, clientPort, sequenceNumber, ackNumber, syn: true, ack: true);
                SendPacket(clientPort, synAckPacket);

                NetworkSegment handshakeConfirmationPacket = ReadPacket(clientPort);

                if (!handshakeConfirmationPacket.ACKFlag || handshakeConfirmationPacket.Data.Length > 0)
                    throw new Exception("Invalid connection confirmation packet from client.");

                Console.WriteLine("\nClient connected successfully. Beginning message transmission.\n");

                // Message transmission
                StringBuilder messageBuilder = new StringBuilder();
                for (int i = 0; i < 10; i++)
                {
                    messageBuilder.Append(i).Append(", ");
                }

                byte[] messageData = messageBuilder.ToString().ToUtf32Bytes();
                List<NetworkSegment> messagePackets = NetworkSegment.GetPackets(messageData, windowSize, _port, clientPort, sequenceNumber, ackNumber).ToList();

                foreach (NetworkSegment packetToSend in messagePackets)
                {
                    Thread.Sleep(10);
                    SendPacket(clientPort, packetToSend);

                    NetworkSegment acknowledgmentPacket = ReadPacket(clientPort);

                    if (!acknowledgmentPacket.ACKFlag || acknowledgmentPacket.Data.Length > 0)
                        throw new Exception("Incorrect acknowledgment packet received from client.");

                    if (acknowledgmentPacket.RSTFlag)
                    {
                        Console.WriteLine($"Emergency connection termination with client {acknowledgmentPacket.SourcePort}.\nReceived packet: {acknowledgmentPacket}");
                        _clients[acknowledgmentPacket.SourcePort].Close();
                        return;
                    }

                    if (packetToSend.SequenceNumber + windowSize != acknowledgmentPacket.AcknowledgmentNumber)
                        throw new Exception("Client acknowledged an incorrect set of data.");
                }

                // Connection teardown
                SendPacket(clientPort, NetworkSegment.CreateEmptySegment(windowSize, _port, clientPort, sequenceNumber, ackNumber, fin: true));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing client {clientPort}: {ex.Message}");
                // Typically, you might want to re-throw the exception or handle it accordingly.
            }
            finally
            {
                // Close socket upon completion of client handling
                if (_clients[clientPort].Connected)
                    _clients[clientPort].Close();
            }
        }
        private void SendPacket(int port, NetworkSegment packet)
        {
            byte[] responseBuffer = packet.ToJsonBytes();
            _clients[port].Send(responseBuffer);
        }

        public NetworkSegment ReadPacket(int port)
        {
            // Буфер для хранения данных
            byte[] messageBuffer = new byte[4096];
            List<byte> res = new();

            // Читаем данные из клиента
            int bytesRead = _clients[port].Receive(messageBuffer);
            res.AddRange(messageBuffer[0..bytesRead]);

            return res.ToArray().DeserializeTcpPacket();
        }
    }
}
