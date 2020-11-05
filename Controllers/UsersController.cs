using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookStoreAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;
        public UsersController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Route("register")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            try
            {
                var username = userDTO.EmailAddress;
                var pass = userDTO.Password;
                var user = new IdentityUser { Email = username, UserName = username };
                var result = await _userManager.CreateAsync(user, pass);

                if (!result.Succeeded)
                {
                    return StatusCode(500, "Something goes wrong");
                }
                await _userManager.AddToRoleAsync(user, "Customer");
                return Created("login",new { result.Succeeded });
            }
            catch (Exception)
            {

                return StatusCode(500, "Something goes wrong");
            }
        }
        /// <summary>
        /// User Login EndPoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Route("login")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            try
            {
                var username = userDTO.EmailAddress;
                var pass = userDTO.Password;
                var result = await _signInManager.PasswordSignInAsync(username, pass, false, false);
                
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(username);
                    var tokenstring = await GenerateJSONWebToken(user);
                    return Ok(new { token = tokenstring });
                }
                return Unauthorized(userDTO);
            }
            catch (Exception)
            {

                return StatusCode(500, "Something goes wrong");
            }
            
        }
        private async Task<string> GenerateJSONWebToken(IdentityUser user)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credential = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r)));
            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Issuer"], claims, null, expires: DateTime.Now.AddMinutes(5), signingCredentials: credential);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
