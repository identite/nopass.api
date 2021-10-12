namespace NoPassIntegrationExample.Contracts.Models.PortalCommunicationModels
{
    public class AuthorizedUserInputModel
    {
        public string AuthId { get; set; }
        public bool IsAuthorized { get; set; }
        public string Reason { get; set; }
    }
}
