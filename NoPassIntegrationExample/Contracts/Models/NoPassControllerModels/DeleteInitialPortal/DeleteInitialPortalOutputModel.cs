namespace NoPassIntegrationExample.Contracts.Models.NoPassControllerModels.DeleteInitialPortal
{
    // Authentication Bearer %AuthToken%
    //authToken is received from server during registration (see description for /api/PortalCommunication/ConfirmRegistration endpoint for details).
    public class DeleteInitialPortalOutputModel
    {
        public string PortalId { get; set; }
        public string UserId { get; set; }
    }
}
