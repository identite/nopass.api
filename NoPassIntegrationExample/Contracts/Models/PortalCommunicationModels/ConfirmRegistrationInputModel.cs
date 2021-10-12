namespace NoPassIntegrationExample.Contracts.Models.PortalCommunicationModels
{
    public class ConfirmRegistrationInputModel
    {
        public string Settings { get; set; }
        public string PortalId { get; set; }
        public string AuthToken { get; set; }
    }
}
