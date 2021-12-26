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

        [HttpGet]
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


        [Authorize(Roles = Role.Admin)]
        [Route("edituser")]
        [HttpPost]
        public IActionResult editUser([FromBody] User user)
        {
            Console.WriteLine(user.Roles);
            try
            {
                var update = _context.Users.FirstOrDefault(e => e.Id == user.Id);
                if (update != null)
                {
                    update.Name = user.Name;
                    update.Email = user.Email;
                    update.Password = user.Password;
                    update.Roles = user.Roles;
                }
                else
                {
                    this.response = new ResponseMessage(0, "false", 403, "Something Went Wrong");
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
            this.response = new ResponseMessage(0, "true", 200, "Product Updated Successfully");
            return Ok(this.response);
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
                    this.response = new ResponseMessage(0, "false", 403, "Something Went Wrong");
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
                this.response = new ResponseMessage(0, "false", 200, "Something Went Wrong");
                return NotFound(this.response);
            }
            this.response = new ResponseMessage(0, "true", 200, "User Register Successfully");
            return Ok(this.response);
        }

        [HttpPost]
        [Route("login")]
        public IActionResult login([FromBody] LoginModel content)
        {
            try
            {
                user =  _context.Users.Where(u => u.Email == content.Email && u.Password == content.Password).FirstOrDefault();
                Models.AuthenticationToken authenticationToken = null;
                if (user != null)
                {
                    authenticationToken = new Models.AuthenticationToken(user);
                }
                else 
                { 
                    this.response = new ResponseMessage(0, "false", 404, "Email or Password is incorrect");
                    return Ok(this.response);
                }
                if (authenticationToken == null)
                {
                    this.response = new ResponseMessage(0, "false", 404, "Email or Password is incorrect");
                    return Ok(this.response);
                }
                authenticationToken.AccessToken = GenerateAccessToken(authenticationToken);
                authenticationToken.responseBool = "true";
                return Ok(authenticationToken);

            }
            catch(Exception e)
            {
                this.response = new ResponseMessage(0, "false", 404, "Something Went Wrong");
                return Ok(this.response);
            }
        }

        [Authorize(Roles = Role.Admin)]
        [Route("deleteuser/{id}")]
        [HttpPost]
        public IActionResult deleteProduct(int id)
        {
            try
            {
                var itemToRemove = _context.Users.SingleOrDefault(x => x.Id == id);
                if (itemToRemove != null)
                {
                    _context.Users.Remove(itemToRemove);
                    _context.SaveChanges();
                }
                else
                {
                    this.response = new ResponseMessage(0, "false", 200, "User Not Found.");
                    return NotFound(this.response);
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.response = new ResponseMessage(0, "false", 200, "Something Went Wrong");
                return NotFound(this.response);
            }
            this.response = new ResponseMessage(0, "true", 200, "User Deleted Successfully");
            return Ok(this.response);
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
