using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using outDO.Data;
using outDO.Models;
using Task = outDO.Models.Task;

namespace outDO.Controllers
{
    public class TaskController : Controller
    {
        //pas 10 user si roluri


        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        public TaskController(ApplicationDbContext context, UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            db = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public IActionResult New(string id)
        {
            ViewBag.BoardId = id;

            return View();
        }

        [HttpPost]
        public IActionResult New([FromForm] Task task)
        {
            string id = Guid.NewGuid().ToString();
            task.Id = id;

            if (ModelState.IsValid)
            {
                db.Tasks.Add(task);
                db.SaveChanges();

                return Redirect("/Board/Show/" + task.BoardId);
            }
            else
            {
                ViewBag.BoardId = task.BoardId;
                return View(task);
            }
        }
    }
}
