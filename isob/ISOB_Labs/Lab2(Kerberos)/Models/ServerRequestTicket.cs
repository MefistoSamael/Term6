namespace Kerberos.Models
{
    internal class ServerRequestTicket(string serviceSessionKey, string principal, int duration)
    {
        public string ServiceSessionKey { get; set; } = serviceSessionKey;
        //Метка времени
        public DateTime TimeStamp { get; init; } = DateTime.Now;
        //Срок жизни
        public int Duration { get; set; } = duration;
        public string Principal { get; set; } = principal;
    }
}
