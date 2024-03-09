using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kerberos.Models
{
    public record class AuthenticationServerResponse(string UserPrincipal,
        byte[] TGSEncryptByClientKey,
        byte[] TGSEncryptByKDCKey);

    public record class TicketGrantServerResponse(string ServicePrincipal,
        byte[] STEncryptBySessionKey,
        byte[] STEncryptByServiceKey);

    public record class ApplicationServerResponse(byte[] ServiceResEncryptByServiceSessionKey);
}
