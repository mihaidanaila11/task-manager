using Microsoft.AspNetCore.Mvc;
using outDO.Data;
using outDO.Models;
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
    }
}
