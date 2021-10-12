namespace NoPassIntegrationExample.Contracts.Models.PortalCommunicationModels
{
    public class UpdatePictureInputModel
    {
        public string AuthId { get; set; }
        public string Image { get; set; }
        public int NextChange { get; set; }
    }
}
