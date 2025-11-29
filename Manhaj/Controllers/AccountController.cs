using Manhaj.Models;
using Manhaj.Services;
using Manhaj.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Manhaj.Controllers
{
    public class AccountController : Controller
    {
        private readonly ManhajDbContext _context;
        private readonly IFileService _fileService;

        public AccountController(ManhajDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if user exists in any of the user tables
            User? user = await _context.Students.FirstOrDefaultAsync(u => u.Email == model.Email);
            string userRole = "Student";

            if (user == null)
            {
                user = await _context.Teachers.FirstOrDefaultAsync(u => u.Email == model.Email);
                userRole = "Teacher";
            }

            if (user == null)
            {
                user = await _context.Admins.FirstOrDefaultAsync(u => u.Email == model.Email);
                userRole = "Admin";
            }

            if (user == null || !PasswordHasher.VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                return View(model);
            }

            if (userRole == "Teacher")
            {
                var teacher = (Teacher)user;
                if (!teacher.IsApproved)
                {
                    ModelState.AddModelError(string.Empty, "حسابك في انتظار موافقة المسؤول");
                    return View(model);
                }
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, userRole)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect based on role
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return userRole switch
            {
                "Student" => RedirectToAction("Dashboard", "Student"),
                "Teacher" => RedirectToAction("Dashboard", "Teacher"),
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم بالفعل");
                return View(model);
            }

            // Check if email is blacklisted
            var isBlacklisted = await _context.Blacklist.AnyAsync(b => b.Email == model.Email);
            if (isBlacklisted)
            {
                ModelState.AddModelError("Email", "هذا البريد الإلكتروني محظور من التسجيل");
                return View(model);
            }

            // Hash password
            var hashedPassword = PasswordHasher.HashPassword(model.Password);

            // Create user based on type
            if (model.UserType == "Student")
            {
                if (string.IsNullOrEmpty(model.Level))
                {
                    ModelState.AddModelError("Level", "المستوى الدراسي مطلوب للطلاب");
                    return View(model);
                }

                var student = new Student
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = hashedPassword,
                    Phone = model.Phone,
                    Level = model.Level
                };

                _context.Students.Add(student);
            }
            else if (model.UserType == "Teacher")
            {
                if (string.IsNullOrEmpty(model.Specialization))
                {
                    ModelState.AddModelError("Specialization", "التخصص مطلوب للمعلمين");
                    return View(model);
                }

                if (model.NationalIdFront == null || model.NationalIdBack == null)
                {
                     ModelState.AddModelError("", "يجب رفع صور البطاقة الشخصية (الوجه والظهر)");
                     return View(model);
                }

                string frontPath = await _fileService.SaveFileAsync(model.NationalIdFront, "teachers_ids");
                string backPath = await _fileService.SaveFileAsync(model.NationalIdBack, "teachers_ids");

                var teacher = new Teacher
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = hashedPassword,
                    Phone = model.Phone,
                    Specialization = model.Specialization,
                    IsApproved = false,
                    NationalIdFrontPath = frontPath,
                    NationalIdBackPath = backPath
                };

                _context.Teachers.Add(teacher);
            }
            else
            {
                ModelState.AddModelError("UserType", "نوع المستخدم غير صحيح");
                return View(model);
            }

            await _context.SaveChangesAsync();

            if (model.UserType == "Teacher")
            {
                TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح! سيقوم المسؤول بمراجعة طلبك والموافقة عليه قريباً.";
            }
            else
            {
                TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح! يمكنك تسجيل الدخول الآن";
            }
            
            return RedirectToAction(nameof(Login));
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Student")
            {
                var student = await _context.Students.FindAsync(userId);
                ViewData["UserRole"] = "طالب";
                return View(student);
            }
            else if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FindAsync(userId);
                ViewData["UserRole"] = "معلم";
                return View(teacher);
            }
            else if (role == "Admin")
            {
                var admin = await _context.Admins.FindAsync(userId);
                ViewData["UserRole"] = "مسؤول";
                return View(admin);
            }

            return NotFound();
        }
    }
}
