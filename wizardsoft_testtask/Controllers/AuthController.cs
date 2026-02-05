using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using wizardsoft_testtask.Dtos;
using wizardsoft_testtask.Data;
using wizardsoft_testtask.Models;
using LoginRequest = wizardsoft_testtask.Dtos.LoginRequest;
using wizardsoft_testtask.Service.Auth;
using System.Threading.Tasks;
using wizardsoft_testtask.Exceptions;

namespace wizardsoft_testtask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var result = await _service.Login(request);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
        {
            var result = await _service.Register(request);
            return Ok(result);
        }
    }
}
