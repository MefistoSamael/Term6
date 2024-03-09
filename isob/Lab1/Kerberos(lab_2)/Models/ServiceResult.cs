using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kerberos.Models
{
    internal class ServiceResult(string message)
    {
        //Метка времени
        public DateTime TimeStamp { get; init; } = DateTime.Now;

        public string Message { get; set; } = message;
    }
}
