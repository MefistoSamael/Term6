using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kerberos
{
    public static class Configuration
    {
        public const int ClientPort = 7000;
        public const int AuthenticationPort = 7001;
        public const int TicketGrantingServicePort = 7002;
        public const int ServiceServerPort = 7003;

        public const string KDCKey =        "keydistributrkey";
        public const string SessionKey =    "sessionkeysessio";
        public const string RequestedServerKey =    "servicekeyservic";
        public const string ClientKey =     "clientkeyclientk";
        public const string ServiceSessionKey = "servicesessionke";

        public static readonly int MaxDuration = 300;

        public static readonly IPEndPoint AuthEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AuthenticationPort);
        public static readonly IPEndPoint TGServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), TicketGrantingServicePort);
        public static readonly IPEndPoint ServiceServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), ServiceServerPort);
    }
}
