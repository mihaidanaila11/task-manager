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

        public CommentController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            Comment comment = db.Comments.Where(c => c.Id == id).First();

            Task task = db.Tasks.Where(t => t.Id == comment.TaskId).First();

            db.Comments.Remove(comment);
            db.SaveChanges();

            return Redirect("/Task/Show/" + task.Id);
        }

        public IActionResult Edit(string id, [FromForm] Comment formComment)
        {
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
