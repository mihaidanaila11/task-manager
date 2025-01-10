using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
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

        private bool isUserAuthorized(string taskId)
        {
            var userId = from t in db.Tasks
                         join b in db.Boards on
                         t.BoardId equals b.Id
                         join p in db.Projects on
                         b.ProjectId equals p.Id
                         join pm in db.ProjectMembers on
                         p.Id equals pm.ProjectId
                         where t.Id == taskId
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
            if(!isUserAuthorized(id))
            {
                return StatusCode(403);
            }

            Task task = db.Tasks.Find(id);
            string boardId = task.BoardId;
            db.Tasks.Remove(task);
            db.SaveChanges();

            return Redirect("/Board/Show/" + boardId);
        }

        [Authorize]
        public IActionResult Edit(string id)
        {
            if (!isUserAuthorized(id))
            {
                return StatusCode(403);
            }
            
            Task task = db.Tasks.Find(id);
            ViewBag.Task = task;

            //membrii proiectului care nu sunt deja assigned la task
            /*var ProjectMembers = (from t in db.Tasks
                          join b in db.Boards on
                          t.BoardId equals b.Id
                          join pm in db.ProjectMembers on
                            b.ProjectId equals pm.ProjectId
                            join u in db.Users on
                            pm.UserId equals u.Id
                                  where t.Id == id
                          where !t.TaskMembers.Contains(pm.User)
                            select new
                            { u.Id, u.UserName, u.Email }).ToList();

            ViewBag.ProjectMembers = ProjectMembers;*/


            return View(task);
        }

        [Authorize, HttpPost]
        public IActionResult Edit(string id, [FromForm] Task requestTask)
        {
            Task task = db.Tasks.Find(id);

            try
            {
                task.Title = requestTask.Title;
                task.Description = requestTask.Description;
                task.Status = requestTask.Status;
                task.DateStart = requestTask.DateStart;
                task.DateFinish = requestTask.DateFinish;
                db.SaveChanges();
                return Redirect("/Board/Show/" + task.BoardId);
            }
            catch (Exception)
            {
                ViewBag.Project = task;

                return View();
            }
        }
    }
}
