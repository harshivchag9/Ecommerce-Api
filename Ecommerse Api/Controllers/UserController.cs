using Ecommerse_Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ProjectApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerse_Api.Controllers
{
    [Route("api/Users")]
    public class UserController : Controller
    {
        private readonly EcomContext _context;
        private readonly JWTSettings _jwtsettings;
        private User user;
        public UserController(EcomContext dbContext, IOptions<JWTSettings> jwtsettings)
        {
            _context = dbContext;
            _jwtsettings = jwtsettings.Value;
        }
        public IActionResult Index()
        {
            return View();  
        }

        [Route("register")]
        [HttpPost]
        public IActionResult register([FromBody] User user)
        {
            try
            {
            _context.Users.Add(user);
            _context.SaveChanges();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return Ok(user);
        }

        [HttpPost]
        [Route("login")]
        public IActionResult login([FromBody] LoginModel content)
        {
            user =  _context.Users.Where(u => u.Email == content.Email && u.Password == content.Password).FirstOrDefault();


            Models.AuthenticationToken authenticationToken = null;

            if (user != null)
            {
                authenticationToken = new Models.AuthenticationToken(user);
            }

            if (authenticationToken == null)
            {
                return NotFound();
            }

            authenticationToken.AccessToken = GenerateAccessToken(authenticationToken);
            return Ok(authenticationToken);
        }
        private string GenerateAccessToken(Models.AuthenticationToken auth)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Email", Convert.ToString(auth.Email))
                     //new Claim(ClaimTypes.Role, auth.Roles)
                    //new Claim("CompanyId", Convert.ToString(auth.RefreshToken))
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
