using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NoPassIntegrationExample.Services.Contracts;

namespace NoPassIntegrationExample.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IRegistrationNoPassService registrationNoPassService;
        private readonly ILogger<ExternalLoginModel> logger;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IRegistrationNoPassService registrationNoPassService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
            this.registrationNoPassService = registrationNoPassService;
        }

        [FromQuery(Name = "token")]
        public string Token { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Check for the presence of registrationNoPassModel by token received from the query string.
        /// Check for the presence of a user and authenticate him.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var registrationNoPassModel = registrationNoPassService.GetModelByToken(Token);
            if (registrationNoPassModel != null && registrationNoPassModel.Confirm)
            {
                var user = await userManager.FindByNameAsync(registrationNoPassModel.Login);
                
                if (user != null)
                {
                    await signInManager.SignInAsync(user, false);
                    registrationNoPassService.RemoteModelByToken(Token);

                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "User not created.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Registration error.");
            return Page();
        }
    }
}
