using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using outDO.Data;
using outDO.Models;

namespace outDO.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        //pas 10 user si roluri


        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        public ProjectController(ApplicationDbContext context, UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            db = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

             public IActionResult Index()
        {
            var projects = db.Projects.ToList();

            ViewBag.Projects = projects;
            return View();
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public IActionResult New([FromForm] Project project)
        {
            string id = Guid.NewGuid().ToString();
            project.Id = id;

            if(ModelState.IsValid)
            {
                db.Projects.Add(project);

                /*
                 * Chestia asta o sa functioneze dupa ce combinam cu
                 * chestia ta luca in care se foloseste tabela de users (cred... sper...)
                 * 
                ProjectMember projectMember = new ProjectMember();

                projectMember.ProjectId = id;
                projectMember.UserId = userManager.GetUserId(User).ToString();
                projectMember.ProjectRole = string.Empty;

                db.ProjectMembers.Add(projectMember);
                */

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                // Console.WriteLine(ex.ToString());
                return View();
            }
        }

        public IActionResult Show(string id)
        {
            Project project = db.Projects.Where(p => p.Id == id).First();

            var boards = db.Boards.Where(b => b.ProjectId == project.Id).ToList();
            ViewBag.Boards = boards;

            return View(project);
        }

    }
}
