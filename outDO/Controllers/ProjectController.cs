using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using outDO.Data;
using outDO.Models;

namespace outDO.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;

        public ProjectController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
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

    }
}
