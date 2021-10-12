using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class ExternalLoginSignalRModel : PageModel
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ILoginNoPassService loginNoPassService;
        private readonly ILogger<ExternalLoginModel> logger;

        public ExternalLoginSignalRModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger,
            ILoginNoPassService loginNoPassService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
            this.loginNoPassService = loginNoPassService;
        }

        [FromQuery(Name = "token")]
        public string Token { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Check for the presence of loginNoPassModel and confirmation by token received from the query string.
        /// Check the presence of a user and authenticate him.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var loginNoPassModel = loginNoPassService.GetModel(Token);
            if (loginNoPassModel != null && loginNoPassModel.Confirm)
            {
                var user = await userManager.FindByEmailAsync(loginNoPassModel.Login);
                if (user != null)
                {
                    await signInManager.SignInAsync(user, loginNoPassModel.RememberMe);
                    loginNoPassService.RemoteModelByToken(Token);

                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Login error.");
            return Page();
        }
    }
}
