
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LemonAuction.Identity;
using Microsoft.AspNetCore.Authorization;
using LemonAuction.DTO;
using System.IO;
using LemonAuction.Services.Exceptions;

namespace LemonAuction.Controllers {
    [Authorize]
    [ApiController]
    [Middleware.ExceptionMiddelware]
    [Route("[controller]/[action]")]
    public class AccountController : Controller {
        private readonly SignInManager<LemonUser> _signInManager;
        private readonly UserManager<LemonUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            SignInManager<LemonUser> signInManager,
            UserManager<LemonUser> userManager,
            IConfiguration configuration
        ) {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<object> Login([FromBody] LoginDto loginData) {
            if (ModelState.IsValid) {
                // TODO: Remember me?
                var result = await _signInManager.PasswordSignInAsync(loginData.Username, loginData.Password, false, false);
                if (result.Succeeded) {
                    var lemonUser = _userManager.Users.SingleOrDefault(r => r.UserName == loginData.Username);
                    return GenerateJwtToken(lemonUser);
                }
                throw new LemonInvalidException("Invalid credentials");
            } else {
                throw new LemonInvalidException("Invalid login values;");
            }
        }

        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Avatar([FromRoute] string userId) {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.Avatar == null) {
                    return NotFound();
                }
                return File(user.Avatar, "image/png");
            }
            return BadRequest("Invalid userId");

        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<object> Register([FromForm] RegisterDto registerData) {
            if (ModelState.IsValid) {

                var user = new LemonUser {
                    UserName = registerData.UserInfo.Username,
                    DateCreated = DateTime.UtcNow
                };
                if (registerData.Avatar != null) {
                    if (string.Equals(registerData.Avatar.ContentType, "image/png", StringComparison.OrdinalIgnoreCase)) {
                     using (var memoryStream = new MemoryStream()) {
                        await registerData.Avatar.CopyToAsync(memoryStream);
                        user.Avatar = memoryStream.ToArray();
                     }
                  }

                    
                }
                user.Email = registerData.UserInfo.Email;
                var result = await _userManager.CreateAsync(user, registerData.UserInfo.Password);
                if (result.Succeeded) {
                    
                    await _signInManager.SignInAsync(user, false);
                    return  GenerateJwtToken(user);
                }
                var exceptionText = result.Errors.Aggregate("User Creation Failed, Errors were: \n\r\n\r", (current, error) => current + (" - " + error.Description + "\n\r"));
                throw new LemonInvalidException(exceptionText);

            } else {
                throw new LemonInvalidException("Bad new user data");
            }
            
        }


        private string GenerateJwtToken(LemonUser user) {
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var securityKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Guid"]));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(claims: claims, signingCredentials: creds);

            return  new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}