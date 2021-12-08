using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerse_Api.Models
{
    public partial class AuthenticationToken : User
    {
        public string AccessToken { get; set; }
        public ClaimsIdentity Role { get; internal set; }

        public AuthenticationToken(User user)
        {
            this.Email = user.Email;
            this.Roles = user.Roles;
        }
    }
}
