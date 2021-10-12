using Microsoft.AspNetCore.SignalR;
using NoPassIntegrationExample.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoPassIntegrationExample.Hubs
{
    /// <summary>
    /// Required for SignalR to work
    /// </summary>
    public class CommunicationHub : Hub
    {
        private readonly ILoginNoPassService loginNoPassService;

        public CommunicationHub(ILoginNoPassService loginNoPassService)
        {
            this.loginNoPassService = loginNoPassService;
        }

        /// <summary>
        /// Set connectionId by authId for further notification of the user.
        /// </summary>
        /// <param name="authId"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public async Task SetConnectionID(string authId)
        {
            if (!string.IsNullOrEmpty(authId))
            {
                var loginNoPassModel = loginNoPassService.GetModel(authId);

                if (loginNoPassModel != null)
                {
                    loginNoPassModel.ConnectionIdSignalR = Context.ConnectionId;
                }
            }
           
            await Task.CompletedTask;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
