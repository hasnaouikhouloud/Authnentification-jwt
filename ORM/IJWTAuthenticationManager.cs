using Microsoft.AspNetCore.Identity;
using Modele;
using ORM.ModelEmail;
using ORM.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ORM
{
    public interface IJWTAuthenticationManager
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable< ApplicationUser> GetAll();
        ApplicationUser GetById(string id);
        void SendEmail(Message message);
        

    }
}
