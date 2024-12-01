using Foodie.Models;
using Foodie.SecurityProvider;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Foodie.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly FoodieDbContext _context;
        private readonly IDataProtector _protector;
        public HomeController(ILogger<HomeController> logger,IWebHostEnvironment env,FoodieDbContext context,IDataProtectionProvider provider,DataSecurityProvider data)
        {
            _logger = logger;
            _env = env;
            _context = context;
            _protector = provider.CreateProtector(data.Seckey);
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
           
        }
        [HttpPost]
        public IActionResult Register(UserEdit edit)
        {
            try
            {
                if(edit.Email != null)
                {
                    var IsEmailexist =_context.Users.Where(x=>x.Email==edit.Email).Any();
                    if (IsEmailexist)
                    {
                        ModelState.AddModelError("", "Email already exist");
                        return View(edit);
                    }
                }
                if (edit.Username != null)
                {
                    var IsUserNamexist = _context.Users.Where(x => x.Username == edit.Username).Any();
                    if (IsUserNamexist)
                    {
                        ModelState.AddModelError("", "Username already exist try another one");
                        return View(edit);
                    }
                }
                if (edit.ProfileImg != null)
                {
                    var file = Guid.NewGuid() + "_" + Path.GetExtension(edit.ProfileImg.FileName);
                    var path = Path.Combine(_env.WebRootPath, "ProfileImages", file);
                    using FileStream str = new FileStream(path, FileMode.Create);
                    edit.ProfileImg.CopyTo(str);
                    edit.ImageUrl = file;
                }
                else
                {
                    ModelState.AddModelError("", "Profile Image is required");
                    return View(edit);
                }
                var ProtectedPass = _protector.Protect(edit.Passwrod);
                User u = new()
                {
                    Name = edit.Name,
                    Username = edit.Username,
                    Mobile = edit.Mobile,
                    Email = edit.Email,
                    Passwrod = ProtectedPass,
                    ImageUrl = edit.ImageUrl,
                    UserRole = "User",
                    Address = edit.Address,
                };

                _context.Add(u);
                _context.SaveChanges();
                return Json("Success");
            }
            catch (Exception err)
            {
                ModelState.AddModelError("", $"Error: {err.Message}");
                return View(edit);
            }

        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return PartialView("_PasswordChange");
        }
        [HttpPost]
        public IActionResult ChangePassword(ChangePass pass)
        {
            if(pass.NewPassword != null||pass.ConfirmPassword!=null||pass.CurrentPassword!=null)
            {
               var user=_context.Users.Where(u=>u.UserId==Convert.ToInt32(User.Identity!.Name)).FirstOrDefault();
                if(user!=null)
                {
                    var unprotectedPass=_protector.Unprotect(user.Passwrod!.ToString());
                    if (pass.CurrentPassword == unprotectedPass)
                    {
                        if (pass.NewPassword == pass.ConfirmPassword)
                        {
                            user.Passwrod=_protector.Protect(pass.NewPassword!);
                            _context.Users.Update(user);
                            _context.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Confirm password doesn't match");
                            return PartialView("_PasswordChange", pass);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please re-chake you current password");
                        return PartialView("_PasswordChange", pass);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User Not found");
                    return PartialView("_PasswordChange", pass);
                }
            }
            else
            {
                ModelState.AddModelError("", "All fields are required");
                return PartialView("_PasswordChange", pass);
            }
        }
        public IActionResult UserProfile()
        {
            var user=_context.Users.Where(u=>u.UserId==Convert.ToInt32(User.Identity!.Name)).FirstOrDefault();
            //var UserId = _protector.Protect(user!.UserId.ToString());
            UserEdit u = new()
            {
                UserId= user!.UserId,
                Name = user!.Name,
                Username = user.Username!,
                Email = user.Email!,
                Address = user.Address!,
                Mobile = user.Mobile!,
                ImageUrl=user.ImageUrl!,
                UserRole=user.UserRole!,

            };
            return View(u);
        }
        [HttpPost]
        public IActionResult ProfileEdit(UserEdit edit)
        {
            var user=_context.Users.Where(u=>u.UserId==Convert.ToInt32(User.Identity!.Name)).FirstOrDefault();
            if (user != null)
            {
                if (edit != null)
                {
                    if (edit.ProfileImg != null)
                    {


                        var fileName = Guid.NewGuid() + "_" + Path.GetExtension(edit.ProfileImg.FileName);
                        var ImgPath = Path.Combine(_env.WebRootPath, "ProfileImages",fileName);
                        var directoryPath =Path.GetDirectoryName(ImgPath);
                        using FileStream str = new FileStream(ImgPath, FileMode.Create);
                        {
                            edit.ProfileImg.CopyTo(str);
                        }
                        user.ImageUrl = fileName;
                        
                    }
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    return Json("Successfully update");

                }
                else
                {
                    ModelState.AddModelError("", "Please provide all required fild");
                    return View(edit);

                }
                
            }
            else
            {
                ModelState.AddModelError("", "user not Found");
                return View(edit);
            }
            
        }

        [HttpGet]
        public IActionResult DashBoard()
        {
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
