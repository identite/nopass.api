using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoPassIntegrationExample.Core.Settings;
using NoPassIntegrationExample.Hubs;
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
    /// Service for working with tokens required for user registration
    /// </summary>
    class LoginNoPassService : ILoginNoPassService
    {
        ConcurrentDictionary<string, LoginNoPassModel> LoginsNoPass { get; set; }
        Timer timer;
        private readonly NoPassSettings settings;
        private readonly ILogger<LoginNoPassService> logger;
        private readonly IHubContext<CommunicationHub> hubContext;

        public LoginNoPassService(ILogger<LoginNoPassService> logger,
            IOptions<NoPassSettings> settings,
            IHubContext<CommunicationHub> hubContext)
        {
            LoginsNoPass = new ConcurrentDictionary<string, LoginNoPassModel>();

            timer = new Timer(DoWork, new Object(), TimeSpan.Zero,  TimeSpan.FromSeconds(settings.Value.CleaningTokenFrequencySeconds));
            this.settings = settings.Value;
            this.logger = logger;
            this.hubContext = hubContext;
        }

        public LoginNoPassModel GetModel(string token)
        {
            if (LoginsNoPass.TryGetValue(token, out var loginNoPassModel))
            {
                return loginNoPassModel;
            }
            return null;
        }

        public bool SetModel(string token, LoginNoPassModel loginNoPassModel)
        {
            return LoginsNoPass.TryAdd(token, loginNoPassModel);
        }

        public bool RemoteModelByToken(string token)
        {
            if (token != null)
            {
                return LoginsNoPass.TryRemove(token, out var _);
            }
            return false;
        }

        /// <summary>
        /// Cleaning up obsolete tokens
        /// </summary>
        /// <param name="state"></param>
        async void DoWork(object state)
        {
            var listToDelete = LoginsNoPass.Where(x => x.Value.CreationTime <= DateTime.UtcNow.AddSeconds(-settings.LifetimeLoginNoPassModelSeconds))
                .ToList();

            if (listToDelete != null)
            {
                foreach (var loginModel in listToDelete)
                {
                    await hubContext.Clients.Client(loginModel.Value.ConnectionIdSignalR).SendAsync("ShowError", "Time is up, please try logging in again");
                    LoginsNoPass.TryRemove(loginModel.Key, out var _);
                }
            }
        }
    }
}
