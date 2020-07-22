using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Modele;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ORM.Models
{
    public class AuthenticateResponse
    {
        

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<ApplicationUser> Role { get; set; }
        public string JwtToken { get; set; }


        public AuthenticateResponse(ApplicationUser user, string jwtToken , List<ApplicationUser> role)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.UserName;
            Email = user.Email;
            Role =role;
            JwtToken = jwtToken;
           
        }

    }
}
