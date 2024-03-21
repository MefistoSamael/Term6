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
            //Создаем сокет и привязывааем к конкретному адресу
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Bind(_ip);

            try
            {
                //Подключаемся к серверу
                clientSocket.Connect(serverIP);


                //Наачинаем отправлять пакеты для подтверждения соединения
                SendPacket(clientSocket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, serverIP.Port, 0, 0, syn: true)); 
                
                NetworkSegment packet = ReadPacket(clientSocket);

                if (!packet.SYNFlag || !packet.ACKFlag)
                    throw new Exception("Incorrect initial packet from server");

                SendPacket(clientSocket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, serverIP.Port, 1, 1, ack: true));

                //Подключение установлено. Теперь читаем сообщение

                StringBuilder serverMessage = new();

                packet = ReadPacket(clientSocket);
                do
                {
                    serverMessage.Append(packet.Data.Utf32BytesToString());

                    Thread.Sleep(200);
                    SendPacket(clientSocket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, serverIP.Port, 1, packet.SequenceNumber + (uint)packet.Data.Length, ack: true));
                    packet = ReadPacket(clientSocket);
                }
                while (!packet.FINFlag);

                Console.WriteLine("\n======================================\n" +
                                  "Message received from server: " + serverMessage +
                                  "\n======================================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error while working with the server: {ex.Message}");
            }
            finally
            {
                if (clientSocket.Connected)
                    clientSocket.Close();
            }

            cancelTokenSource.Cancel();
            return Task.CompletedTask;
        }

        private void SendPacket(Socket socket, NetworkSegment packet)
        {
            byte[] responseBuffer = packet.ToJsonBytes();
            socket.Send(responseBuffer);
        }

        public NetworkSegment ReadPacket(Socket socket)
        {
            // Буфер для хранения данных
            byte[] messageBuffer = new byte[4096];
            List<byte> res = new();

            // Читаем данные из клиента
            int bytesRead = socket.Receive(messageBuffer);
            res.AddRange(messageBuffer[0..bytesRead]);

            return res.ToArray().DeserializeTcpPacket();
        }
    }
}
