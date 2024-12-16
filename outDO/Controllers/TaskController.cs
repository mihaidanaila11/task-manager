using Microsoft.AspNetCore.Mvc;
using outDO.Data;
using outDO.Models;
using Task = outDO.Models.Task;

namespace outDO.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext db;

        public TaskController(ApplicationDbContext db)
        {
            this.db = db;
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
