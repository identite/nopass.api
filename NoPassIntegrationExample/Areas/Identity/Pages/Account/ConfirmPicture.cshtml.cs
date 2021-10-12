using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoPassIntegrationExample.Contracts.Models.NoPassControllerModels.RequestAuthorization;
using NoPassIntegrationExample.Core.Settings;
using NoPassIntegrationExample.Services.Contracts;
using NoPassIntegrationExample.Services.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoPassIntegrationExample.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmPictureModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<LoginModel> logger;
        private readonly ILoginNoPassService loginNoPassService;
        private readonly NoPassSettings settings;

        public ConfirmPictureModel(ILogger<LoginModel> logger,
            IHttpClientFactory httpClientFactory,
            ILoginNoPassService loginNoPassService,
            IOptions<NoPassSettings> settings)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.loginNoPassService = loginNoPassService;
            this.settings = settings.Value;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [FromQuery(Name = "login")]
        public string Login { get; set; }

        [FromQuery(Name = "rememberMe")]
        public bool RememberMe { get; set; }

        public string AuthId { get; set; }
        public int NextChange { get; set; }
        public string ImageData { get; set; }
        public bool isShowImage { get; set; }

        /// <summary>
        /// Get the image, AuthId, NextChange from NoPass and display the image on the page.
        /// Also on this page the confirmPicture.js file will be loaded which will manage the page state.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            var urlRequestAuthorization = $"{settings.NoPassServerURL}/api/UserAuthentication/RequestAuthorization";

            var outputModel = new RequestAuthorizationOutputModel()
            {
                PortalId = settings.PortalId,
                UserId = Login
            };

            try
            {
                var httpClient = httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.AuthToken);

                using var response = await httpClient.PostAsJsonAsync(urlRequestAuthorization, outputModel);
                
                response.EnsureSuccessStatusCode();
                var requestAuth = await response.Content.ReadAsStringAsync();

                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var inputModel = JsonSerializer.Deserialize<RequestAuthorizationInputModel>(requestAuth, serializeOptions);

                if (inputModel != null && inputModel.Errors.Length == 0)
                {
                    if (inputModel.Errors.Length != 0)
                    {
                        throw new Exception();
                    }

                    AuthId = inputModel.Result.AuthId;
                    NextChange = inputModel.Result.NextChange - 1000;

                    var loginNoPassModel = new LoginNoPassModel() { Login = Login, RememberMe = RememberMe, CreationTime = DateTime.UtcNow };

                    //  Save the received token and data to loginNoPassService.
                    var result = loginNoPassService.SetModel(inputModel.Result.AuthId, loginNoPassModel);
                    if (result)
                    {
                        ImageData = String.Format("data:image/png;base64,{0}", inputModel.Result.Image);
                        isShowImage = true;
                        return Page();
                    }
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error receiving picture");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Error receiving picture");
            return Page();
        }
    }
}
