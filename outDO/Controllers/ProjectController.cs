using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using outDO.Data;
using outDO.Models;
using Project = outDO.Models.Project;

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

        private bool isUserAuthorized(string projectId)
        {
            var userId = from p in db.Projects
                         join pm in db.ProjectMembers on
                         p.Id equals pm.ProjectId
                         where p.Id == projectId
                         select pm.UserId;
            if (userId.First() != userManager.GetUserId(User))
            {
                return false;
            }

            return true;
        }

        [Authorize]
        public IActionResult Delete(string id)
        {
            if (!isUserAuthorized(id))
            {
                return StatusCode(403);
            }

            Project project = db.Projects.Find(id);

            db.Projects.Remove(project);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Edit(string id)
        {
            if (!isUserAuthorized(id))
            {
                return StatusCode(403);
            }

            Project project = db.Projects.Find(id);
            ViewBag.Project = project;

            return View();
        }

        [Authorize, HttpPost]
        public IActionResult Edit(string id, [FromForm] Project requestProject)
        {
            Project project = db.Projects.Find(id);

            try
            {
                project.Name = requestProject.Name;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.Project = project;

                return View();
            }
        }
    }
}
