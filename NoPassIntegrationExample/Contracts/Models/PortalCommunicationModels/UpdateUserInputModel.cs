namespace NoPassIntegrationExample.Contracts.Models.PortalCommunicationModels
{
    public class UpdateUserInputModel
    {
        public string UserId { get; set; }
        public string PortalId { get; set; }
        public Updates Updates { get; set; }
    }
}
