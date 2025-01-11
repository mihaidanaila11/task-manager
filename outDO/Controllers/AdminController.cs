using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using outDO.Data;
using outDO.Models;

namespace outDO.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;
        public AdminController(ApplicationDbContext context, UserManager<User> _userManager,
            RoleManager<IdentityRole> _roleManager, IWebHostEnvironment env)
        {
            db = context;
            userManager = _userManager;
            roleManager = _roleManager;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            ViewBag.Users = userManager.Users;
            return View();
        }

        public IActionResult Projects()
        {
            ViewBag.Projects = db.Projects;
            return View();
        }

        public IActionResult GoBack()
        {
            return RedirectToAction("Index");
        }
    }
}
