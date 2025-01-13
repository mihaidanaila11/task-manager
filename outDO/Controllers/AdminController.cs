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
        private readonly SignInManager<User> signInManager;

        public AdminController(ApplicationDbContext context, UserManager<User> _userManager,
            RoleManager<IdentityRole> _roleManager, IWebHostEnvironment env, SignInManager<User> _signInManager)
        {
            db = context;
            userManager = _userManager;
            roleManager = _roleManager;
            _env = env;
            signInManager = _signInManager;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {

            //toti adminii  inafara de user ul curent
            var admins = userManager.GetUsersInRoleAsync("Admin").Result;
            //inafara de user ul curent
            admins.Remove(userManager.GetUserAsync(User).Result);

            ViewBag.admins = admins;

            //user ii inafar de admini
            var nonAdmins = from u in db.Users
                        where !admins.Contains(u)
                        where u.Id != userManager.GetUserId(User)
                        select u;


            //userii inafara de admini + banned status
            List<Tuple<User, bool>> users = new List<Tuple<User, bool>>();
            List<Tuple<User, bool>> bannedUsers = new List<Tuple<User, bool>>();

            foreach (var user in nonAdmins)
            {
                if (db.BannedEmails.Where(b => b.email == user.UserName).ToList().Count > 0)
                {
                    bannedUsers.Add(new Tuple<User, bool>(user, true));
                }
                else
                {
                    users.Add(new Tuple<User, bool>(user, false));
                }
            }

            ViewBag.Users = users;
            ViewBag.bannedUsers = bannedUsers;

            return View();
        }

        public IActionResult Projects()
        {
            Dictionary<string, List<User>> projectOrg = new Dictionary<string, List<User>>();//lista de organizatori pentru fiecare proiect

            foreach (var project in db.Projects)
            {
                List<User> projectOrganisers = new List<User>();

                foreach (var projectMember in db.ProjectMembers)
                {
                    if (projectMember.ProjectId == project.Id && projectMember.ProjectRole == "Organizator")
                    {
                        projectOrganisers.Add(db.Users.Find(projectMember.UserId));
                    }
                }
                projectOrg.Add(project.Id, projectOrganisers);
            }

            ViewBag.ProjectOrganisers = projectOrg;

            ViewBag.Projects = db.Projects;
            return View();
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


        public IActionResult AddAdmin(string id, bool banned)
        {
            if (banned == false)
            {

                User user = userManager.FindByIdAsync(id).Result;
                userManager.AddToRoleAsync(user, "Admin").Wait();
                return RedirectToAction("Users");
            }
            else
            {
               TempData["AdminUnbanUser"] = "The user must be unbanned before naming as admin.";
                return RedirectToAction("Users");
            }
        }

        public IActionResult RemoveAdmin(string id)
        {
            if (id != userManager.GetUserId(User))
            {
                return Redirect("/Identity/Account/AccessDenied");
            }

            var admins = userManager.GetUsersInRoleAsync("Admin").Result;

            if(admins.Count() == 1)
            {
                TempData["AdminAlertMessage"] = "The website must have at least one administrator. Assign a different administrator if you wish to step down.";
                return RedirectToAction("Users");
            }

            User user = userManager.FindByIdAsync(id).Result;
            userManager.RemoveFromRoleAsync(user, "Admin").Wait();
            //log off the user
            signInManager.SignOutAsync().Wait();
            return Redirect("/Home/Index");
        }

        public IActionResult DeleteUser(string id)
        {
            User user = userManager.FindByIdAsync(id).Result;
            userManager.DeleteAsync(user).Wait();
            return RedirectToAction("Users");
        }

       
    }
}
