using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using outDO.Data;
using outDO.Models;

namespace outDO.Controllers
{
    public class BoardController : Controller
    {
        //pas 10 user si roluri
        

        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        public BoardController(ApplicationDbContext context,UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            db = context;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        [Authorize]
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

        [Authorize]
        public IActionResult Show(string id)
        {
            Board board = db.Boards.Where(b => b.Id == id).First();

            var tasks = db.Tasks.Where(t => t.BoardId == board.Id).ToList();
            ViewBag.Tasks = tasks;

            return View(board);
        }

        private bool isUserAuthorized(string boardId)
        {
            var userId = from b in db.Boards
                         join p in db.Projects on
                         b.ProjectId equals p.Id
                         join pm in db.ProjectMembers on
                         p.Id equals pm.ProjectId
                         where b.Id == boardId
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

            Board board = db.Boards.Find(id);

            string projectId = board.ProjectId;
            db.Boards.Remove(board);
            db.SaveChanges();

            return Redirect("/Project/Show/" + projectId);
        }

        [Authorize]
        public IActionResult Edit(string id)
        {
            if(!isUserAuthorized(id))
            {
                return StatusCode(403);
            }

            Board board = db.Boards.Find(id);
            ViewBag.Board = board;

            return View();
        }

        [Authorize, HttpPost]
        public IActionResult Edit(string id, [FromForm] Board requestBoard)
        {
            Board board = db.Boards.Find(id);

            try
            {
                board.Name = requestBoard.Name;
                db.SaveChanges();
                return Redirect("/Project/Show/" + board.ProjectId);
            }
            catch (Exception)
            {
                ViewBag.Project = board;

                return View();
            }
        }

        public IActionResult GoBack(string id)
        {   //ne intoarcem la proiectul din care am venit
            Board board = db.Boards.Find(id);
            return Redirect("/Project/Show/" + board.ProjectId);
        }
    }
}
