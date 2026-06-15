namespace DemoApp.Web.Models.Shared
{
    public static class ValidationPatterns
    {
        public const string AlphanumericId = @"^[a-zA-Z0-9]{5,20}$";
        public const string PhoneNumber = @"^\+?\d{7,15}$";
        public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    }

}
