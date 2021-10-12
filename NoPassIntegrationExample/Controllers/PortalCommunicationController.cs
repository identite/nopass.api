using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoPassIntegrationExample.Core.Settings;
using NoPassIntegrationExample.Contracts.Models.PortalCommunicationModels;
using NoPassIntegrationExample.Hubs;
using NoPassIntegrationExample.Services.Contracts;
using NoPassIntegrationExample.Services.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoPassIntegrationExample.Controllers
{
    [Route("api/[controller]")]
    public partial class PortalCommunicationController : Controller
    {
        private readonly ILogger logger;
        private readonly IRegistrationNoPassService registrationNoPassService;
        private readonly IHubContext<CommunicationHub> hubContext;
        private readonly ILoginNoPassService loginNoPassService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly NoPassSettings settings;

        public PortalCommunicationController(ILogger<PortalCommunicationController> logger,
            IRegistrationNoPassService registrationNoPassService,
            IHubContext<CommunicationHub> hubContext,
            ILoginNoPassService loginNoPassService,
            UserManager<IdentityUser> userManager,
            IOptions<NoPassSettings> settings)
        {
            this.logger = logger;
            this.registrationNoPassService = registrationNoPassService;
            this.hubContext = hubContext;
            this.loginNoPassService = loginNoPassService;
            this.userManager = userManager;
            this.settings = settings.Value;
        }

        /// <summary>
        /// This method is first called when a portal is registered to verify whether the admin login is correct.
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        [HttpPost(nameof(ConfirmPreRegistration))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ConfirmPreRegistrationOutputModel))]
        public async Task<IActionResult> ConfirmPreRegistration([FromBody] ConfirmPreRegistrationInputModel inputModel)
        {
            if (inputModel != null)
            {
                var outputModel = new ConfirmPreRegistrationOutputModel()
                {
                    AdminId = settings.RegistrationAdminId,
                    SCode = settings.RegistrationSCode,
                    R = inputModel.R + 1
                };

                if (outputModel.AdminId == inputModel.AdminId)
                {
                    return Ok(outputModel);
                }
            }
          
            return BadRequest();
        }

        /// <summary>
        /// This method is called after the administrator has finished registering for NoPass.
        /// The received data must be saved and used next time you with NoPass.
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        [HttpPost(nameof(ConfirmRegistration))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ConfirmRegistrationOutputModel))]
        public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationInputModel inputModel)
        {
            if (inputModel != null)
            {
                var outputModel = new ConfirmRegistrationOutputModel()
                {
                    SCode = settings.RegistrationSCode
                };

                //For this example, the data is output to the console and saved directly to the settings.
                logger.LogInformation("----------------------------------");
                logger.LogInformation($"PortalId: {inputModel.PortalId}");
                logger.LogInformation($"Settings: {inputModel.Settings}");
                logger.LogInformation($"AuthToken: {inputModel.AuthToken}");
                logger.LogInformation("----------------------------------");

                settings.AuthToken = inputModel.AuthToken;
                settings.PortalId = inputModel.PortalId;
                return Ok(outputModel);
            }

            return BadRequest();
        }

        /// <summary>
        /// This method is called when the user has registered on the system.
        /// If the OTP is in registrationNoPassService and the user is not in the database, then NoPass saves the new user.
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        [HttpPost(nameof(ConfirmUserRegistration))] 
        [SwaggerResponse(StatusCodes.Status200OK, Type = null)]
        public async Task<IActionResult> ConfirmUserRegistration([FromBody] ConfirmUserRegistrationInputModel inputModel)
        {
            if (inputModel != null)
            {
                var registrationNoPassModel = registrationNoPassService.GetModel(inputModel.Otp);
                if (registrationNoPassModel != null)
                {
                    var user = await userManager.FindByEmailAsync(registrationNoPassModel.Login);
                    if (user == null)
                    {
                        var identityUser = new IdentityUser { UserName = registrationNoPassModel.Login, Email = registrationNoPassModel.Login };
                        var result = await userManager.CreateAsync(identityUser);

                        if (result.Succeeded)
                        {
                            registrationNoPassModel.Confirm = true;
                            return Ok();
                        }
                        return BadRequest();
                    }
                    registrationNoPassModel.Confirm = true;
                    return Ok();
                }
            }
            
            return BadRequest();
        }

        /// <summary>
        /// This method is called when the images received from the NoPass system in the process of authorization are updated.
        /// If there is a loginNoPassModel, then send new data to the frontend in SignalR.
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        [HttpPost(nameof(UpdatePicture))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null)]
        public async Task<IActionResult> UpdatePicture([FromBody] UpdatePictureInputModel inputModel)
        {
            if (inputModel != null)
            {
                var loginNoPassModel = loginNoPassService.GetModel(inputModel.AuthId);
                if (loginNoPassModel != null)
                {
                    await hubContext.Clients.Client(loginNoPassModel.ConnectionIdSignalR).SendAsync("ChangeImage", inputModel.Image, inputModel.NextChange);
                    return Ok();
                }
            }

            return BadRequest();
        }

        /// <summary>
        /// This method is called when the authorization process is completed.
        /// If the token is in registrationNoPassService and inputModel.IsAuthorized, then NoPassredirects to the login page.
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        [HttpPost(nameof(AuthorizedUser))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null)]
        public async Task<IActionResult> AuthorizedUser([FromBody] AuthorizedUserInputModel inputModel)
        {
            if (inputModel != null)
            {
                var loginNoPassModel = loginNoPassService.GetModel(inputModel.AuthId);
                if (loginNoPassModel != null && inputModel.IsAuthorized)
                {
                    loginNoPassModel.Confirm = true;
                    await hubContext.Clients.Client(loginNoPassModel.ConnectionIdSignalR).SendAsync("Redirect", $"{settings.ThisPortalURL}/Identity/Account/ExternalLoginSignalR?token=", inputModel.AuthId);
                    return Ok();
                }

                await hubContext.Clients.Client(loginNoPassModel?.ConnectionIdSignalR).SendAsync("ShowError", inputModel.Reason);
                loginNoPassService.RemoteModelByToken(inputModel.AuthId);
            }
            
            return BadRequest();
        }

        /// <summary>
        /// This method is called when the NoPass server is attempting to check whether this user can be created on the portal side.
        /// RegistrationNoPassModel is created and saved, which will help to save the user in the future if it is registered on the NoPass server.
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        [HttpPost(nameof(ValidateUserRegistration))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<IActionResult> ValidateUserRegistration([FromBody] ValidateUserRegistrationInputModel inputModel)
        {
            if (inputModel != null)
            {
                var timedUser = new RegistrationNoPassModel() { Login = inputModel.Login, CreationTime = DateTime.UtcNow };

                var result = registrationNoPassService.SetModel(inputModel.Otp, timedUser);
                if (result)
                {
                    return Ok(true);
                }
                return Ok(false);
            }
            return BadRequest();
        }

        /// <summary>
        /// This method indicates that the user account in NoPass was deleted.
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        [HttpPost(nameof(DeleteUser))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null)]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserInputModel inputModel)
        {
            if (inputModel != null && inputModel.PortalId == settings.PortalId)
            {
                var user = await userManager.FindByEmailAsync(inputModel.UserId);
                if (user != null)
                {
                    var result = await userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return Ok();
                    }
                }
            }

            return BadRequest();
        }

        /// <summary>
        /// This method is called to check id there is a possibility to update user's information.
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        [HttpPost(nameof(UpdateUser))]
        [SwaggerResponse(StatusCodes.Status200OK, Type = null)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserInputModel inputModel)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
    }
}
