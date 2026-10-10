namespace IdentityAuthLayer.Infrastructure.Email
{
    public class SendGridSettings
    {
        public string? ApiKey { get; set; }
        public string? SecretKey { get; set; }
        public string SenderEmail { get; set; } = "ahmdasamhabwnar@gmail.com";
        public string SenderName { get; set; } = "Identity Auth Layer";

        public string EffectiveApiKey => !string.IsNullOrWhiteSpace(SecretKey) ? SecretKey : ApiKey ?? string.Empty;
    }
}
