using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using wizardsoft_testtask.Dtos;
using LoginRequest = wizardsoft_testtask.Dtos.LoginRequest;

namespace wizardsoft_testtask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtOptions _jwtOptions;

        private static readonly IReadOnlyDictionary<string, (string Password, string Role)> Users = new Dictionary<string, (string Password, string Role)>
        {
            ["admin"] = ("admin_password", "Admin"),
            ["maxsmg"] = ("qweqwe", "User")
        };

        public AuthController(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login(LoginRequest request)
        {
            if (!Users.TryGetValue(request.UserName, out var userInfo))
            {
                return Unauthorized();
            }

            if (userInfo.Password != request.Password)
            {
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptions.Key);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.UserName),
            new Claim(ClaimTypes.Role, userInfo.Role)
        };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = credentials
            });

            var jwt = tokenHandler.WriteToken(token);

            return Ok(new LoginResponse(jwt, request.UserName, userInfo.Role));
        }
    }
}
