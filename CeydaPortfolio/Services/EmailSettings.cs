namespace CeydaPortfolio.Services
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 465;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}