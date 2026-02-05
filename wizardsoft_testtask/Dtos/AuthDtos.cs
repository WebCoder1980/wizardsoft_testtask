namespace wizardsoft_testtask.Dtos
{
    public record LoginRequest(string UserName, string Password);
    public record LoginResponse(string Token, string UserName, string Role);
    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int ExpiresMinutes { get; set; }
    }
}
