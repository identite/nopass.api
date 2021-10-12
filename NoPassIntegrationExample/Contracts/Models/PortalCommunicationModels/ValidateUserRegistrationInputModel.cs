namespace NoPassIntegrationExample.Contracts.Models.PortalCommunicationModels
{
    public class ValidateUserRegistrationInputModel
    {
        public string Otp { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}
