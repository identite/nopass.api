using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoPassIntegrationExample.Core.Settings;
using NoPassIntegrationExample.Contracts.Models.NoPassControllerModels.PreRegisterUser;
using NoPassIntegrationExample.Services.Contracts;
using NoPassIntegrationExample.Services.Models;

namespace NoPassIntegrationExample.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterNoPassModel : PageModel
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ILogger<RegisterNoPassModel> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IRegistrationNoPassService registrationNoPassService;
        private readonly NoPassSettings settings;
        string remoteIp;

        public RegisterNoPassModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterNoPassModel> logger,
            IHttpClientFactory httpClientfactory,
            IRegistrationNoPassService registrationNoPassService,
            IOptions<NoPassSettings> settings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.httpClientFactory = httpClientfactory;
            this.registrationNoPassService = registrationNoPassService;
            this.settings = settings.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        /// <summary>
        /// We receive email(login) from the form and check if the user is in the database.
        /// If there is no user, then we pre-register the user for NoPass and redirect him to the NoPass website.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var urlPreRegisterUser = $"{settings.NoPassServerURL}/api/UserRegistration/PreRegisterUser";
            var guidToken = Guid.NewGuid().ToString();

            var user = await userManager.FindByEmailAsync(Input.Email); 
            if (user == null)
            {
                var outputModel = new PreRegisterUserOutputModel()
                {
                    PortalId = settings.PortalId,
                    UserId = Input.Email,
                    ClientIP = remoteIp,
                    // The address to which the lalin will be made and the user is saved after registering on the NoPass website.
                    RedirectUrl = $"{settings.ThisPortalURL}/Identity/Account/ExternalLogin?token={guidToken}",
                    SocialNetwork = null,
                    Data = new NoPassIntegrationExample.Contracts.Models.NoPassControllerModels.PreRegisterUser.Data()
                    {
                        Email = Input.Email,
                        GivenName = null,
                        PhoneNumber = null,
                        SurName = null
                    }
                };

                try
                {
                    var httpClient = httpClientFactory.CreateClient();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.AuthToken);

                    using var response = await httpClient.PostAsJsonAsync(urlPreRegisterUser, outputModel);
                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadAsStringAsync();

                    var serializeOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var inputModel = JsonSerializer.Deserialize<PreRegisterUserInputModel>(result, serializeOptions);

                    if (inputModel != null)
                    {
                        if (inputModel.Errors.Length != 0)
                        {
                            throw new Exception();
                        }

                        var registrationNoPassModel = new RegistrationNoPassModel() { Login = Input.Email, AuthToken = guidToken, CreationTime = DateTime.UtcNow };

                        // Save OTP password and registrationNoPassModel received from NoPass.
                        registrationNoPassService.SetModel(inputModel.Result.Otp, registrationNoPassModel);

                        return Redirect(inputModel.Result.RegisterLink);
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Registration error");
                    return Page();
                }
            }

            ModelState.AddModelError(string.Empty, "Email is already assigned with registered account");
            return Page();
        }

        /// <summary>
        /// Get the client's IP address. If a proxy server is used, you need to receive data through javascript.
        /// </summary>
        /// <param name="context"></param>
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
            if (remoteIp != null && remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }
            logger.LogInformation("Remote IpAddress: {RemoteIp}", remoteIp);
            this.remoteIp = remoteIp?.ToString();
        }
    }
}
