using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Lab3_WebApp.Models
{
    public static class IdentitySeedData
    {
        public static async void EnsurePopulated(IApplicationBuilder app)
        {
            UserManager<AppUser> userManager = app.ApplicationServices.GetRequiredService<UserManager<AppUser>>();

            AppUser admin = new AppUser
            {
                UserName = "admin555",
                Email = "admin@mail.ca",
                Firstname = "Administrator",
                Lastname = "Support"
            };

            const string admin_password = "Secret123$";

            AppUser found = await userManager.FindByEmailAsync(admin.Email);

            if (found == null)
            {
                await userManager.CreateAsync(admin, admin_password);
            }
        }
    }
}
