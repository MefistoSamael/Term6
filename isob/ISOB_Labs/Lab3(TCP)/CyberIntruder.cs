using System.Net;
using System.Net.Sockets;

namespace Lab3
{
    internal class CyberIntruder
    {
        public async Task ConnectToServer(IPEndPoint clientIP, CancellationToken token)
        {
            await Task.Delay(500);

            //SynFloodAttack();
            //ResetAttack(token);
        }

        private void ResetAttack(CancellationToken token)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                clientSocket.Connect(NetworkNode.ServerIP);

                SendPacket(clientSocket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, NetworkNode.ServerIP.Port, 0, 0, syn: true));

                NetworkSegment packet = ReadPacket(clientSocket);

                if (!packet.SYNFlag || !packet.ACKFlag)
                    throw new Exception("Incorrect first packer from server");

                SendPacket(clientSocket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, NetworkNode.ServerIP.Port, 1, 1, ack: true));

                //Подключение установлено. Теперь пытаемся разорвать соединение

                for(int i = 5; i < 100; i++)
                {
                    if (token.IsCancellationRequested)
                        break;

                    Thread.Sleep(100);
                    try
                    {
                        //Отправляем пакеты с флагом RST и подставляем порт нашего клиента, типа это он отправил
                        SendPacket(clientSocket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, NetworkNode.ServerIP.Port, (uint)i * 4 + 1, 1, rst: true, ack: true));
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while intruder worked with server: {ex.Message}");
                //throw;
            }
            finally
            {
                if (clientSocket.Connected)
                    clientSocket.Close();
            }
        }

        //Просто спамим сообщениями о том, что хотим подключиться
        private void SynFloodAttack()
        {
            Parallel.For(5, 20, (int i) =>
            {
                try
                {
                    Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(NetworkNode.ServerIP);

                    SendPacket(socket, NetworkSegment.CreateEmptySegment(4, Constants.ClientPort, 1000 + i, 0, 0, syn: true));
                }
                catch { }
            });
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
            int bytesRead;

            // Читаем данные из клиента
            bytesRead = socket.Receive(messageBuffer);
            res.AddRange(messageBuffer[0..bytesRead]);

            return res.ToArray().DeserializeTcpPacket();
        }
    }
}
