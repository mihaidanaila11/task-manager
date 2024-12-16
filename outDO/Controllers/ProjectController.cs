using Microsoft.AspNetCore.Authorization;
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
        private readonly UserManager<User> userManager;

        public ProjectController(
            ApplicationDbContext db,
            UserManager<User> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            var projects = from pm in db.ProjectMembers
                           join p in db.Projects on
                           pm.ProjectId equals p.Id
                           where pm.UserId == userManager.GetUserId(User)
                           select p;

            ViewBag.Projects = projects;
            return View();
        }

        [Authorize]
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

                ProjectMember projectMember = new ProjectMember();

                projectMember.ProjectId = id;
                projectMember.UserId = userManager.GetUserId(User).ToString();
                projectMember.ProjectRole = string.Empty;

                db.ProjectMembers.Add(projectMember);

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                // Console.WriteLine(ex.ToString());
                return View();
            }
        }

        [Authorize]
        public IActionResult Show(string id)
        {
            Project project = db.Projects.Where(p => p.Id == id).First();

            var boards = db.Boards.Where(b => b.ProjectId == project.Id).ToList();
            ViewBag.Boards = boards;

            return View(project);
        }

    }
}
