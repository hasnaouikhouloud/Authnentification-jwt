using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;

using System.Text;
using System.Text.Json.Serialization;

namespace Modele
{
   public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string ParentUserAdmin { get; set; }

        public DateTime LastConnection { get; set; }

        public DateTime ReseredPassword { get; set; }

       


    }
}
