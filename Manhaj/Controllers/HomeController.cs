using System.Diagnostics;
using Manhaj.Models;
using Microsoft.AspNetCore.Mvc;

namespace Manhaj.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ManhajDbContext _context;

        public HomeController(ILogger<HomeController> logger, ManhajDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.CoursesCount = _context.Courses.Count();
            ViewBag.TeachersCount = _context.Teachers.Count();
            ViewBag.StudentsCount = _context.Students.Count();
            return View();
        }

        public IActionResult Privacy()
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
