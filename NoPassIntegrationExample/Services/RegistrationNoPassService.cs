using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoPassIntegrationExample.Core.Settings;
using NoPassIntegrationExample.Services.Contracts;
using NoPassIntegrationExample.Services.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NoPassIntegrationExample.Services
{
    /// <summary>
    /// Service for working with tokens required for user login
    /// </summary>
    public class RegistrationNoPassService : IRegistrationNoPassService
    {
        ConcurrentDictionary<string, RegistrationNoPassModel> RegistrationsNoPass { get; set; }
        Timer timer;
        private readonly NoPassSettings settings;
        private readonly ILogger<RegistrationNoPassService> logger;

        public RegistrationNoPassService(ILogger<RegistrationNoPassService> logger,
            IOptions<NoPassSettings> settings)
        {
            RegistrationsNoPass = new ConcurrentDictionary<string, RegistrationNoPassModel>();

            timer = new Timer(DoWork, new Object(), TimeSpan.Zero, TimeSpan.FromSeconds(settings.Value.CleaningTokenFrequencySeconds));
            this.settings = settings.Value;
            this.logger = logger;
        }

        public RegistrationNoPassModel GetModel(string otp)
        {
            if (RegistrationsNoPass.TryGetValue(otp, out var temporaryUser))
            {
                return temporaryUser;
            }
            return null;
        }

        public RegistrationNoPassModel GetModelByToken(string token)
        {
            var RegistrationsNoPassModel = RegistrationsNoPass.FirstOrDefault(u => u.Value.AuthToken == token);

            if (RegistrationsNoPassModel.Key != null)
            {
                return RegistrationsNoPassModel.Value;
            }
            return null;
        }

        public bool SetModel(string otp, RegistrationNoPassModel temporaryUser)
        {
            return RegistrationsNoPass.TryAdd(otp, temporaryUser);
        }

        public bool RemoteModelByToken(string token)
        {
            var registrationsNoPassModel = RegistrationsNoPass.FirstOrDefault(x => x.Value.AuthToken == token);

            if (registrationsNoPassModel.Key != null)
            {
                return RegistrationsNoPass.TryRemove(registrationsNoPassModel.Key, out var _);
            }
            return false;
        }

        /// <summary>
        /// Cleaning up obsolete tokens
        /// </summary>
        /// <param name="state"></param>
        async void DoWork(object state)
        {
            var listToDelete = RegistrationsNoPass.Where(x => x.Value.CreationTime <= DateTime.UtcNow.AddSeconds(-settings.LifetimeRegistrationNoPassModelSeconds)) 
                .Select(x => x.Key)
                .ToList();

            if (listToDelete != null)
            {
                foreach (var item in listToDelete)
                {
                    RegistrationsNoPass.TryRemove(item, out var _);
                }
            }
        }
    }
}
