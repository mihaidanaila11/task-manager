using Microsoft.AspNetCore.Mvc;
using outDO.Data;
using outDO.Models;

namespace outDO.Controllers
{
    public class BoardController : Controller
    {
        private readonly ApplicationDbContext db;

        public BoardController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult New(string id)
        {
            ViewBag.ProjectId = id;

            return View();
        }

        [HttpPost]
        public IActionResult New([FromForm] Board board)
        {
            string id = Guid.NewGuid().ToString();
            board.Id = id;

            if (ModelState.IsValid)
            {
                db.Boards.Add(board);
                db.SaveChanges();

                return Redirect("/Project/Show/" + board.ProjectId);
            }
            else
            {
                ViewBag.ProjectId = board.ProjectId;
                return View(board);
            }
        }

        public IActionResult Show(string id)
        {
            Board board = db.Boards.Where(b => b.Id == id).First();

            var tasks = db.Tasks.Where(t => t.BoardId == board.Id).ToList();
            ViewBag.Tasks = tasks;

            return View(board);
        }
    }
}
