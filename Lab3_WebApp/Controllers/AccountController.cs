using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab3_WebApp.Models;
using Lab3_WebApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Lab3_WebApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(Account account)
        {
            if(ModelState.IsValid)
            {
                AppUser user = new AppUser()
                {
                    Firstname = account.Firstname,
                    Lastname = account.Lastname,
                    UserName = account.Username,
                    Email = account.Email
                };

                IdentityResult result = await userManager.CreateAsync(user, account.Password);

                if(result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
                    return RedirectToAction("index", "home");
                }
            }

            ModelState.AddModelError("", "Email must be unique. Passwords must contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least six characters long.");
            return View(account);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Login account)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await userManager.FindByEmailAsync(account.Email);

                if (user != null)
                {
                    if ((await signInManager.PasswordSignInAsync(user, account.Password, false, false)).Succeeded)
                    {
                        return Redirect("/Home/Index");
                    }
                }
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(account);
        }

        [HttpGet]
        public async Task<RedirectResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Redirect("/Home/Index");
        }

    }
}