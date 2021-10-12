using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoPassIntegrationExample.Services.Models
{
    public class LoginNoPassModel
    {
        public string ConnectionIdSignalR { get; set; }
        public bool Confirm { get; set; }
        public string Login { get; set; }
        public bool RememberMe { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
