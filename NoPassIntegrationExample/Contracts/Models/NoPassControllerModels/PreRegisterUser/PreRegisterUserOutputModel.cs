namespace NoPassIntegrationExample.Contracts.Models.NoPassControllerModels.PreRegisterUser
{

    // Authentication Bearer%AuthToken% //authToken is received from server during registration (see description for /api/PortalCommunication/ConfirmRegistration endpoint for details).
    public class PreRegisterUserOutputModel
    {
        public string PortalId { get; set; }
        public string UserId { get; set; }
        public string ClientIP { get; set; }
        public string RedirectUrl { get; set; }
        public string SocialNetwork { get; set; }
        public Data Data { get; set; }
    }
}
