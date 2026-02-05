using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using wizardsoft_testtask.Constants;
using wizardsoft_testtask.Data;
using wizardsoft_testtask.Dtos;
using wizardsoft_testtask.Exceptions;
using wizardsoft_testtask.Models;

namespace wizardsoft_testtask.Service.Auth
{
    public class AuthService : IAuthService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly AppDbContext _dbContext;

        public AuthService(IOptions<JwtOptions> jwtOptions, AppDbContext dbContext)
        {
            _jwtOptions = jwtOptions.Value;
            _dbContext = dbContext;
        }
        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == request.UserName);
            if (user == null)
            {
                throw new InvalidCredentialsException();
            }

            var passwordHash = AuthUtil.HashPassword(request.Password);
            if (!string.Equals(user.PasswordHash, passwordHash, StringComparison.Ordinal))
            {
                throw new InvalidCredentialsException();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptions.Key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, request.UserName),
                new Claim(ClaimTypes.Role, user.Role)
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

            return new LoginResponse(jwt, request.UserName, user.Role);
        }

        public async Task<RegisterResponse?> Register(RegisterRequest request)
        {
            var exists = await _dbContext.Users.AnyAsync(x => x.UserName == request.UserName);
            if (exists)
            {
                throw new UserAlreadyExistsException();
            }

            var user = new User
            {
                UserName = request.UserName,
                PasswordHash = AuthUtil.HashPassword(request.Password),
                Role = AppRoles.USER
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new RegisterResponse(user.UserName, user.Role);
        }
    }
}
