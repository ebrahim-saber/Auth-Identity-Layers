namespace IdentityAuthLayer.Infrastructure.Email
{
    public class MailSettings
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public string SenderEmail { get; set; } = default!;
        public string SenderName { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool UseSsl { get; set; } = true;
    }
}
