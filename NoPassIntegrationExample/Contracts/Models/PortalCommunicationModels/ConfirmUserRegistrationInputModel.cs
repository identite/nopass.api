namespace NoPassIntegrationExample.Contracts.Models.PortalCommunicationModels
{
    public class ConfirmUserRegistrationInputModel
    {
        public string Otp { get; set; }
        public string RegisterLink { get; set; }
    }
}
