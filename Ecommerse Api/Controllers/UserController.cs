using Ecommerse_Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
        private ResponseMessage response;

        public UserController(EcomContext dbContext, IOptions<JWTSettings> jwtsettings)
        {
            _context = dbContext;
            _jwtsettings = jwtsettings.Value;
        }
        public IActionResult Index()
        {
            return View();  
        }

        [Authorize(Roles = Role.Admin)]
        [Route("getusers")]
        [HttpGet]
        public IActionResult getUsers()
        {
            return Ok(_context.Users.ToList());
        }


        [Authorize]
        [Route("getuserdetails")]
        [HttpPost]
        public IActionResult getUSerDetails([FromForm] string email)
        {
            return Ok(_context.Users.Where(x=>x.Email == email).ToList());
        }

        [Authorize]
        [Route("editprofile")]
        [HttpPost]
        public IActionResult updateUsers([FromBody] User currentUser)
        {
            try
            {
                var update = _context.Users.FirstOrDefault(e => e.Id == currentUser.Id);
                if (update != null)
                {
                    update.Name = currentUser.Name;
                    update.Email = currentUser.Email;
                    update.Password = currentUser.Password;
                    update.Roles = "9";
                }
                else
                {
                    this.response = new ResponseMessage(0, "false", 403, "");
                    return NotFound(this.response);
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                this.response = new ResponseMessage(0, "false", 403, "Something Went Wrong");
                Console.WriteLine(e);
                return NotFound(this.response);
            }
            this.response = new ResponseMessage(0, "true", 200, "Data Updated Successfully");
            return Ok(this.response);
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
                return NotFound();
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
                    new Claim("Email", Convert.ToString(auth.Email)),
                    new Claim(ClaimTypes.Role, auth.Roles)
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
