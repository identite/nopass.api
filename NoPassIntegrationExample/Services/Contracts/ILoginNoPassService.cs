using NoPassIntegrationExample.Services.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoPassIntegrationExample.Services.Contracts
{
    public interface ILoginNoPassService
    {
        LoginNoPassModel GetModel(string token);
        bool RemoteModelByToken(string token);
        bool SetModel(string token, LoginNoPassModel loginNoPassModel);
    }
}
