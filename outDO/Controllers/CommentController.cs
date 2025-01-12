using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using outDO.Data;
using outDO.Models;
using System.Threading.Tasks;
using Task = outDO.Models.Task;

namespace outDO.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;
        public CommentController(ApplicationDbContext context, UserManager<User> _userManager,
            RoleManager<IdentityRole> _roleManager, IWebHostEnvironment env)
        {
            db = context;
            userManager = _userManager;
            roleManager = _roleManager;
            _env = env;
        }

        private bool isUserAuthorized(string commentId)
        {   //daca e organizator 
            var userIds = from c in db.Comments
                         join t in db.Tasks
                         on c.TaskId equals t.Id
                         where c.Id == commentId
                         join b in db.Boards on
                         t.BoardId equals b.Id
                         join p in db.Projects on
                         b.ProjectId equals p.Id
                         join pm in db.ProjectMembers on
                         p.Id equals pm.ProjectId
                         select pm.UserId;
            if (!userIds.Any())
            {
                return false;
            }

            if (userIds.Contains(userManager.GetUserId(User)))
            {
                return true;
            }

            return false;
        }

        public bool isUsersComment(string commentId)
        {
            var users = from c in db.Comments
                        where c.Id == commentId
                        select c.UserId;
            if (users.First() != userManager.GetUserId(User))
            {
                return false; //ca sigur e doar unul
            }
            return true;
        }

        //womp womp
        [HttpGet]
        public IActionResult Delete(string id)
        {
            if (!isUserAuthorized(id) && !isUsersComment(id) && !User.IsInRole("Admin"))
            {
                return StatusCode(403);
            }

            Comment comment = db.Comments.Where(c => c.Id == id).First();

            Task task = db.Tasks.Where(t => t.Id == comment.TaskId).First();

            db.Comments.Remove(comment);
            db.SaveChanges();

            return Redirect("/Task/Show/" + task.Id);
        }

        public IActionResult Edit(string id, [FromForm] Comment formComment)
        {
            if (!isUsersComment(id))
            {
                return StatusCode(403);
            }

            Comment comment = db.Comments.Where(c => c.Id == id).First();
            Task task = db.Tasks.Where(t => t.Id == comment.TaskId).First();

            ModelState.Remove(nameof(formComment.Id));
            formComment.Id = comment.Id;

            ModelState.Remove(nameof(formComment.TaskId));
            formComment.TaskId = comment.TaskId;

            ModelState.Remove(nameof(formComment.UserId));
            formComment.UserId = comment.UserId;

            if (TryValidateModel(formComment))
            {
                comment.Content = formComment.Content;
                db.SaveChanges();
            }

            return Redirect("/Task/Show/" + task.Id);
        }
    }
}
