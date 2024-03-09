using Kerberos.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kerberos.Servers
{
    internal class AuthenticationServer
    {
        private readonly string _clientKey = Configuration.ClientKey;
        private readonly string _kdcKey = Configuration.KDCKey;
        private readonly HashSet<string> _users = new HashSet<string> { "Vassya", "Ivan", "Gennadiy", "Auth" };

        public async Task Listen(CancellationToken token)
        {
            using (UdpClient udpClient = new UdpClient(Configuration.AuthEndPoint))
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await udpClient.ReceiveAsync(token);
                    string message = result.Buffer.GetJsonString();
                    IPEndPoint endPoint = result.RemoteEndPoint;

                    ResponseData<AuthenticationServerRequest>? authRequest = JsonSerializer.Deserialize<ResponseData<AuthenticationServerRequest>>(message);

                    if (authRequest is null)
                    {
                        Console.WriteLine("Сервер аутентификации. Не получилось преобразовать полученный запрос");
                        continue;
                    }

                    AuthenticationServerRequest data = authRequest.Data!;

                    // Проверка принципала пользователя
                    if (!_users.Contains(data.UserPrincipal))
                    {
                        byte[] notFound = JsonSerializer.Serialize(
                                new ResponseData<AuthenticationServerResponse>()
                                { IsSuccess = false, ErrorMessage = "Пользователь не найден" }).GetBytes();

                        await udpClient.SendAsync(notFound, endPoint);
                        continue;
                    }

                    string sessionKey = Configuration.SessionKey;

                    int duration = data.Duration < Configuration.MaxDuration
                                    ? data.Duration : Configuration.MaxDuration;

                    TicketGrantingTicket tgt = new(sessionKey, data.UserPrincipal, duration);
                    AuthenticationServerResponse response = new(data.UserPrincipal,
                        JsonSerializer.Serialize(tgt).GetDesEncryptBytes(_clientKey),
                        JsonSerializer.Serialize(tgt).GetDesEncryptBytes(_kdcKey));

                    await udpClient.SendAsync(new ResponseData<AuthenticationServerResponse>() { Data = response, IsSuccess = true }.GetBytes(), endPoint);
                }
            }
        }
    }
}
