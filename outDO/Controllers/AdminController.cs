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
            List<Tuple<User, bool>> users= new List<Tuple<User, bool>>();

            foreach (var user in db.Users)
            {
                if(db.BannedEmails.Where(b => b.email == user.UserName).ToList().Count > 0)
                {
					users.Add(new Tuple<User, bool>(user, true));
				}
                else
                {
					users.Add(new Tuple<User, bool>(user, false));
				}
            }

            ViewBag.Users = users;
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

        public IActionResult BanUser(string id)
        {
            string userName = db.Users.Where(u => u.Id == id).First().UserName;
            if(!db.BannedEmails.Where(b => b.email == userName).Any())
            {
                if (userName != null)
                {
                    BannedEmail bannedEmail = new BannedEmail();
                    bannedEmail.email = userName;
                    db.BannedEmails.Add(bannedEmail);
                    db.SaveChanges();
                }
            }
            
            return RedirectToAction("Users");
        }

        public IActionResult UnBanUser(string id)
        {
			string userName = db.Users.Where(u => u.Id == id).First().UserName;

			if (db.BannedEmails.Where(b => b.email == userName).Any())
			{
				if (userName != null)
				{
                    
					BannedEmail bannedEmail = db.BannedEmails.Where(b => b.email == userName).First();
					
					db.BannedEmails.Remove(bannedEmail);
					db.SaveChanges();
				}
			}

			return RedirectToAction("Users");
		}
    }
}
