using NoPassIntegrationExample.Services.Models;
using System.Collections.Concurrent;

namespace NoPassIntegrationExample.Services.Contracts
{
    public interface IRegistrationNoPassService
    {
        RegistrationNoPassModel GetModel(string otp);
        RegistrationNoPassModel GetModelByToken(string token);
        bool RemoteModelByToken(string token);
        bool SetModel(string otp, RegistrationNoPassModel temporaryUser);
    }
}