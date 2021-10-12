namespace NoPassIntegrationExample.Contracts.Models.PortalCommunicationModels
{
    public class Updates
    {
        public Givenname GivenName { get; set; }
        public Surname SurName { get; set; }
        public Phonenumber PhoneNumber { get; set; }
        public Email Email { get; set; }
        public Profileimageurl ProfileImageUrl { get; set; }
        public Locale Locale { get; set; }
        public Login Login { get; set; }
    }
}