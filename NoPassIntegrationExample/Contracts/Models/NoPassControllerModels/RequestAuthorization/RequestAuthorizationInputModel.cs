namespace NoPassIntegrationExample.Contracts.Models.NoPassControllerModels.RequestAuthorization
{
    public class RequestAuthorizationInputModel
    {
        public Error[] Errors { get; set; }
        public Result Result { get; set; }
        public string LoginUrl { get; set; }
    }
}
