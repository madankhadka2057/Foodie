using Foodie.Models;
using Foodie.SecurityProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Foodie.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly FoodieDbContext _context;
        private IDataProtector _protector;
        public AccountController(FoodieDbContext context,IDataProtectionProvider provider,DataSecurityProvider key )
        {
            _context = context;
            _protector=provider.CreateProtector(key.Seckey);
        }


        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserEdit edit)
        {
            var user =await _context.Users.ToListAsync();
            if (user == null)
            {
                ModelState.AddModelError("", "No user found please register first ");
                return View(edit);
            }
            else
            {
                var loginUser = user.Where(u => u.Email == edit.Email && _protector.Unprotect(u.Passwrod!) == edit.Passwrod).FirstOrDefault();
                if (loginUser != null)
                {
                    List<Claim> claims = new()
                    {
                        new Claim(ClaimTypes.Name,loginUser.UserId.ToString()),
                        new Claim(ClaimTypes.Role,loginUser.UserRole),
                        new Claim("Email",loginUser.Email!),
                        new Claim("FullName",loginUser.Name),
                    };
                    var Identity=new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(Identity));
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(edit);
                }
            }
        }

        [Authorize]
       public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
