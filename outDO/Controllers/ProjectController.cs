using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
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

            if (ModelState.IsValid)
            {
                db.Projects.Add(project);

                ProjectMember projectMember = new ProjectMember();

                projectMember.ProjectId = id;
                projectMember.UserId = userManager.GetUserId(User).ToString();
                projectMember.ProjectRole = "Organizator";

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

        private bool isUserAuthorized(string projectId, string userId) //daca este organizatorul proiectului
        {
            var userIds = from p in db.Projects
                          join pm in db.ProjectMembers on
                          p.Id equals pm.ProjectId
                          where p.Id == projectId
                          where pm.ProjectRole == "Organizator"
                          select pm.UserId;

            if (userIds.Contains(userId))
            {
                return true;
            }

            return false;
        }


        [Authorize]
        public IActionResult Delete(string id, string userId)
        {
            if (!isUserAuthorized(id, userId))
            {
                return StatusCode(403);
            }

            Project project = db.Projects.Find(id);

            db.Projects.Remove(project);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Edit(string id, string userId)
        {
            if (!isUserAuthorized(id, userId))
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

        public IActionResult GoBack()
        {   //ne intoarcem la toate proiectele
            return RedirectToAction("Index");
        }

        public IActionResult AddMembers(string id, string userId)
        {
            var thisUserEmail = (from p in db.Projects
                            join pm in db.ProjectMembers on
                            p.Id equals pm.ProjectId
                            join u in db.Users on
                            pm.UserId equals u.Id
                            where p.Id == id
                            where u.Id == userId //sigur e organizator deci nu mai verific
                            select u.Email).FirstOrDefault();
            ViewBag.ThisUserEmail = thisUserEmail;

            //organizatorii inafara de user ca il pun separat
            var organisers = (from p in db.Projects
                              join pm in db.ProjectMembers on
                              p.Id equals pm.ProjectId
                              join u in db.Users on
                              pm.UserId equals u.Id
                              where p.Id == id
                              where u.Id != userId
                              where pm.ProjectRole == "Organizator"
                              select new
                              { u.Id, u.UserName, u.Email }).ToList();

            ViewBag.Organisers = organisers;

            var members = (from p in db.Projects
                           join pm in db.ProjectMembers on
                           p.Id equals pm.ProjectId
                           join u in db.Users on
                           pm.UserId equals u.Id
                           where p.Id == id
                           where pm.ProjectRole != "Organizator"
                           select new
                           { u.Id, u.UserName, u.Email }).ToList();

            ViewBag.Members = members;

            var allUsers = (from u in db.Users
                            select new
                            { u.Id, u.UserName, u.Email }).ToList();

            var nonMembers = (from u in allUsers
                              where !members.Contains(u)
                              where !organisers.Contains(u)
                              where u.Id != userId
                              select new
                              { u.Id, u.UserName, u.Email }).ToList();

            ViewBag.nonMembers = nonMembers;

            var project = db.Projects.Find(id);
            return View(project);
        }

        public IActionResult AddOrganiser(string id, string userId, string memberId)
        {
            //el e deja membru deci trb sa il gasesc si sa il modific
            var projectMember = db.ProjectMembers.Find(memberId, id);
            projectMember.ProjectRole = "Organizator";
            db.SaveChanges(); 
            return RedirectToAction("AddMembers", new { id = id, userId = userId });
        }

        public IActionResult AddMember(string id, string userId, string memberId)
        {
            ProjectMember projectMember = new ProjectMember();
            projectMember.ProjectId = id;
            projectMember.UserId = memberId;
            db.ProjectMembers.Add(projectMember);
            db.SaveChanges();
            return RedirectToAction("AddMembers", new { id = id, userId = userId });
        }

        public IActionResult RemoveOrganiser(string id, string userId)
        {
            //verificam sa nu fie un singur organizator
            var organisers = (from p in db.Projects
                              join pm in db.ProjectMembers on
                              p.Id equals pm.ProjectId
                              where p.Id == id
                              where pm.ProjectRole == "Organizator"
                              select pm.UserId).ToList();

            if (organisers.Count <= 1)
            {
                TempData["AlertMessage"] = "The project must have at least one organiser. Assign a different organiser if you wish to step down";
                return RedirectToAction("AddMembers", new { id = id, userId = userId });
            }

            var projectMember = db.ProjectMembers.Find(userId, id);
            projectMember.ProjectRole = "Member";
            db.SaveChanges();
            return RedirectToAction("Index");

        }
    
        public IActionResult RemoveMember(string id, string userId, string memberId)
        {
            var projectMember = db.ProjectMembers.Find(memberId, id);
            db.ProjectMembers.Remove(projectMember);
            db.SaveChanges();
            return RedirectToAction("AddMembers", new { id = id, userId = userId });
        }

    }
}
