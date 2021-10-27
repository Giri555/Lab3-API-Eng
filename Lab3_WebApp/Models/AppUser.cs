using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab3_WebApp.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
