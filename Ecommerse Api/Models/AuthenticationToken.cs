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
        public ClaimsIdentity Roles { get; internal set; }

        public AuthenticationToken(User user)
        {
            //this.Id = user.Id;
            this.Email = user.Email;
            //this.Username = user.Username;
            //this.Password = user.Password;
            //this.Phone = user.Phone;
            //this.ProfilePhoto = user.ProfilePhoto;
            //this.Roles = user.Roles;
            //this.DefaultCompanyId = user.DefaultCompanyId;
        }
    }
}
