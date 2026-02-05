using System.ComponentModel.DataAnnotations;

namespace wizardsoft_testtask.Dtos
{
    public record LoginRequest([Length(5, 50)] string UserName, [Length(5, 50)] string Password);
    public record LoginResponse(string Token, string UserName, string Role);
    public record RegisterRequest([Length(5, 50)] string UserName, [Length(5, 50)] string Password);
    public record RegisterResponse(string UserName, string Role);

    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int ExpiresMinutes { get; set; }
    }
}
