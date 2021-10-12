using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoPassIntegrationExample.Core.Settings
{
    public class NoPassSettings
    {
        public string PortalId { get; set; }
        public string AuthToken { get; set; }
        public string RegistrationAdminId { get; init; }
        public string RegistrationSCode { get; init; }
        public string ThisPortalURL { get; init; }
        public string NoPassServerURL { get; init; }
        public int CleaningTokenFrequencySeconds { get; init; }
        public int LifetimeLoginNoPassModelSeconds { get; init; }
        public int LifetimeRegistrationNoPassModelSeconds { get; init; }
    }
}
