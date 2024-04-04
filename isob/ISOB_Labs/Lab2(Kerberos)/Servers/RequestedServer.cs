using System.Net.Sockets;
using System.Net;
using Kerberos.Models;
using System.Text.Json;

namespace Kerberos.Servers
{
    internal class RequestedServer
    {
        private readonly string _message = "It's service response";
        private readonly string _key = Configuration.RequestedServerKey;

        public async Task Listen(CancellationToken token)
        {
            using (UdpClient udpClient = new UdpClient(Configuration.ServiceServerEndPoint))
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await udpClient.ReceiveAsync(token);
                    string message = result.Buffer.GetJsonString();
                    IPEndPoint endPoint = result.RemoteEndPoint;

                    ResponseData<ApplicationServerRequest>? appRequest
                        = JsonSerializer.Deserialize<ResponseData<ApplicationServerRequest>>(message);

                    if (appRequest is null)
                    {
                        Console.Error.WriteLine("Сервер сервиса. Не получилось преобразовать полученный запрос");
                        continue;
                    }

                    ApplicationServerRequest data = appRequest.Data!;

                    ServerRequestTicket tgs;
                    Authenticator userAuth;


                    // Расшифровываем TGT при помощи ключа KDC
                    tgs = JsonSerializer.Deserialize<ServerRequestTicket>(data.TGSEncryptByServiceKey.GetJsonString(_key))!;
                    // Расшифровываем Authenticator при помощи ключа сессии сервиса
                    userAuth = JsonSerializer.Deserialize<Authenticator>(data.AuthEncryptBySessionServiceKey.GetJsonString(tgs.ServiceSessionKey))!;


                    if (tgs.TimeStamp.AddSeconds(tgs.Duration) < DateTime.Now   // Если билет протух
                        || tgs.Principal != userAuth.Principal)                 // Если принципалы не совпадают
                    {
                        byte[] notFound = JsonSerializer.Serialize(
                                new ResponseData<ApplicationServerResponse>()
                                { IsSuccess = false, ErrorMessage = "Билет не действителен" }).GetBytes();

                        await udpClient.SendAsync(notFound, endPoint);
                        continue;
                    }

                    ApplicationServerResponse response = new(JsonSerializer.Serialize(_message).GetDesEncryptBytes(tgs.ServiceSessionKey));

                    await udpClient.SendAsync(new ResponseData<ApplicationServerResponse>() { Data = response, IsSuccess = true }.GetBytes(), endPoint);
                    
                }
            }
        }
    }
}
