using System.Net.Sockets;
using System.Net;
using Kerberos.Models;
using System.Text.Json;

namespace Kerberos.Servers
{
    //Будет выдавать TGS
    internal class TicketGrantingServer
    {
        private const string _key = Configuration.KDCKey;
        private readonly int _baseDuration = Configuration.MaxDuration;

        public async Task Listen(CancellationToken token)
        {
            using (UdpClient udpClient = new UdpClient(Configuration.TGServerEndPoint))
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await udpClient.ReceiveAsync(token);
                    string message = result.Buffer.GetJsonString();
                    IPEndPoint endPoint = result.RemoteEndPoint;

                    ResponseData<TicketGrantServerRequest>? tgsRequest
                        = JsonSerializer.Deserialize<ResponseData<TicketGrantServerRequest>>(message);

                    if (tgsRequest is null)
                    {
                        Console.Error.WriteLine("Сервер выдачи разрешений. Не получилось преобразовать полученный запрос");
                        continue;
                    }

                    TicketGrantServerRequest data = tgsRequest.Data!;

                    string servicePrincipal = data.ServicePrincipal;


                    //Расшифровываем TGT при помощи ключа KDC
                    TicketGrantingTicket tgt = JsonSerializer.Deserialize<TicketGrantingTicket>(data.TGTEncryptByKDC.GetJsonString(_key))!;
                    string sessionKey = tgt.SessionKey;
                    Authenticator userAuthenticator = JsonSerializer.Deserialize<Authenticator>(data.AuthEncryptBySessionKey.GetJsonString(sessionKey))!;

                    if (tgt.TimeStamp.AddSeconds(tgt.Duration) < DateTime.Now   //Если билет протух
                        || tgt.Principal != userAuthenticator.Principal)                 //Если принципалы не совпадают
                    {
                        byte[] notFound = JsonSerializer.Serialize(
                                new ResponseData<TicketGrantServerResponse>()
                                { IsSuccess = false, ErrorMessage = "Билет не действителен" }).GetBytes();

                        await udpClient.SendAsync(notFound, endPoint);
                        continue;
                    }

                    ServerRequestTicket st = new(Configuration.ServiceSessionKey, userAuthenticator.Principal, _baseDuration);

                    byte[] stByServiceKey = JsonSerializer.Serialize(st).GetDesEncryptBytes(_key);
                    byte[] stBySessionKey = JsonSerializer.Serialize(st).GetDesEncryptBytes(sessionKey);

                    TicketGrantServerResponse response = new(servicePrincipal, stBySessionKey, stByServiceKey);

                    await udpClient.SendAsync(new ResponseData<TicketGrantServerResponse>() { Data = response, IsSuccess = true }.GetBytes(), endPoint);
                }
            }
        }
    }
}
