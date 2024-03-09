using Kerberos.Models;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kerberos.Servers
{
    internal class ClientServer
    {
        private const string _key = Configuration.ClientKey;
        private readonly string _login;
        private readonly int _requestedLifeTime;
        private readonly string _servicePrincipal;

        public ClientServer(string login)
        {
            _login = login;
            _requestedLifeTime = 300;
            _servicePrincipal = "Awesome service";
        }

        public async Task Listen(CancellationTokenSource cancelTokenSource)
        {
            UdpClient udpClient = new UdpClient(Configuration.ClientPort);
            
            await SendAuthenticationRequest(udpClient);

            var response = await udpClient.ReceiveAsync();
            var authResponse = DeserializeResponse<ResponseData<AuthenticationServerResponse>>(response.Buffer);

            if (authResponse is null)
            {
                HandleErrorResponse(cancelTokenSource, "Клиент. Ошибка при получении ответа");
                return;
            }

            if (!authResponse.IsSuccess)
            {
                cancelTokenSource.Cancel();
                await Console.Out.WriteLineAsync("Клиент. Неуспешный ответ при аутентификации: " + authResponse.ErrorMessage);
                return;
            }

            var tgtByKDCkey = authResponse.Data!.TGSEncryptByClientKey;
            var tgt = DeserializeTicketGrantingTicket(authResponse.Data.TGSEncryptByClientKey);
            var sessionKey = tgt.SessionKey;

            Console.WriteLine($"\n**********************************************\n" +
                $"Клиент получил ответ от сервера аутентификации." +
                $"\nСессионный ключ: {sessionKey}" +
                $"\n-----------------------------------------------------");

            await SendTGSRequest(udpClient, tgtByKDCkey, sessionKey, cancelTokenSource);
        }

        private async Task SendAuthenticationRequest(UdpClient udpClient)
        {
            var authData = new AuthenticationServerRequest(_login, _requestedLifeTime);
            var authRequest = new ResponseData<AuthenticationServerRequest>() { Data = authData, IsSuccess = true };
            var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authRequest));

            await udpClient.SendAsync(data, Configuration.AuthEndPoint);
            Console.WriteLine("**********************************************\n" +
                
                
                $"Клиент отправил запрос на аутентификацию с принципалом {_login} и запрашиваемым" +
                $" временем жизни {_requestedLifeTime} секунд ({_requestedLifeTime/60} минут)");
        }

        private TicketGrantingTicket DeserializeTicketGrantingTicket(byte[] encryptedData)
        {
            return JsonSerializer.Deserialize<TicketGrantingTicket>(encryptedData.GetJsonString(_key))!;
        }

        private async Task SendTGSRequest(UdpClient udpClient, byte[] tgtByKDCkey, string sessionKey, CancellationTokenSource cancelTokenSource)
        {
            var authenticator = new Authenticator(_login);
            var encryptAuth = JsonSerializer.Serialize(authenticator).GetDesEncryptBytes(sessionKey);

            Console.WriteLine("\n**********************************************\n" +
                "Клиент отправляет запрос на разрешение доступа к сервису (TGS)." +
                $"\nПринципал сервиса: {_servicePrincipal}");

            var tgsRequest = new TicketGrantServerRequest(_servicePrincipal, tgtByKDCkey, encryptAuth);
            await udpClient.SendAsync(new ResponseData<TicketGrantServerRequest>() { Data = tgsRequest, IsSuccess = true }.GetBytes(), Configuration.TGServerEndPoint);

            var response = await udpClient.ReceiveAsync();
            var tgsResponse = DeserializeResponse<ResponseData<TicketGrantServerResponse>>(response.Buffer);

            if (tgsResponse is null )
            {
                HandleErrorResponse(cancelTokenSource, "Клиент. Ошибка при получении ответа");
                return;
            }

            if (!tgsResponse.IsSuccess)
            {
                cancelTokenSource.Cancel();
                await Console.Out.WriteLineAsync("Клиент. Неуспешный ответ при аутентификации: " + tgsResponse.ErrorMessage);
                return;
            }

            var st = DeserializeServiceTicket(tgsResponse.Data!.STEncryptBySessionKey, sessionKey);
            var sessionServiceKey = st.ServiceSessionKey;

            Console.WriteLine("\n**********************************************\n" +
                "Клиент получил ответ от TGS." +
                $"\nСессионный ключ сервиса: {sessionServiceKey}" +
                $"\n-----------------------------------------------------");

            await SendServiceRequest(udpClient, sessionServiceKey, tgsResponse.Data!.STEncryptByServiceKey, cancelTokenSource);
        }

        private ServerRequestTicket DeserializeServiceTicket(byte[] encryptedData, string sessionKey)
        {
            return JsonSerializer.Deserialize<ServerRequestTicket>(encryptedData.GetJsonString(sessionKey))!;
        }

        private async Task SendServiceRequest(UdpClient udpClient, string sessinServiceKey, byte[] encryptST, CancellationTokenSource cancelTokenSource)
        {
            var encryptAuth = JsonSerializer.Serialize(new Authenticator(_login)).GetDesEncryptBytes(sessinServiceKey);
            var serviceRequest = new ApplicationServerRequest(encryptAuth, encryptST);

            Console.WriteLine("\n**********************************************\n" +
                "Клиент отправляет запрос на разрешение доступа к сервису (TGS).");

            await udpClient.SendAsync(new ResponseData<ApplicationServerRequest>() { Data = serviceRequest, IsSuccess = true }.GetBytes(), Configuration.ServiceServerEndPoint);
            var response = await udpClient.ReceiveAsync();

            var appResponse = DeserializeResponse<ResponseData<ApplicationServerResponse>>(response.Buffer);

            if (appResponse is null || !appResponse.IsSuccess)
            {
                await HandleErrorResponseAsync(cancelTokenSource, "Клиент. Ошибка при получении ответа от сервиса");
                return;
            }

            if (!appResponse.IsSuccess)
            {
                cancelTokenSource.Cancel();
                await Console.Out.WriteLineAsync("Клиент. Неуспешный ответ при аутентификации: " + appResponse.ErrorMessage);
                return;
            }

            var message = JsonSerializer.Deserialize<string>(appResponse.Data!.ServiceResEncryptByServiceSessionKey.GetJsonString(sessinServiceKey))!;
            await Console.Out.WriteLineAsync("\n**********************************************\n" +
                "Клиент получил ответ от сервиса." +
                $"\nПолученное сообщение: {message}" +
                $"\n-----------------------------------------------------");
            cancelTokenSource.Cancel();
        }

        private T? DeserializeResponse<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data));
        }

        private async void HandleErrorResponse(CancellationTokenSource cancelTokenSource, string errorMessage)
        {
            cancelTokenSource.Cancel();
            Console.WriteLine(errorMessage);
        }

        private async Task HandleErrorResponseAsync(CancellationTokenSource cancelTokenSource, string errorMessage)
        {
            cancelTokenSource.Cancel();
            await Console.Out.WriteLineAsync(errorMessage);
        }
    }
}
