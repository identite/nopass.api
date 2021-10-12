using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoPassIntegrationExample.Services.Models
{
    public class RegistrationNoPassModel
    {
        public string Login { get; set; }
        public bool Confirm { get; set; }
        public string AuthToken { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
