using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using outDO.Data;
using outDO.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using System.Net.NetworkInformation;
using Task = outDO.Models.Task;

namespace outDO.Controllers
{
    public class TaskController : Controller
    {
        //pas 10 user si roluri


        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;
        public TaskController(ApplicationDbContext context, UserManager<User> _userManager, 
            RoleManager<IdentityRole> _roleManager, IWebHostEnvironment env)
        {
            db = context;
            userManager = _userManager;
            roleManager = _roleManager;
            _env = env;
        }

        [Authorize]
        public IActionResult New(string id)
        {
            ViewBag.BoardId = id;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> New([FromForm] Task task, IFormFile Media)
        {
            string id = Guid.NewGuid().ToString();
            task.Id = id;

            if (Media != null && Media.Length > 0)
            {
                // Verificam extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif"};
                var fileExtension = Path.GetExtension(Media.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "Fisierul trebuie sa fie o imagine (jpg, jpeg, png, gif)");
                    return View(task);
                }

                // Cale stocare

                // Folosim un time signature unic pt fiecare poza
                // pentru a avea nume diferite la fiecare upload
                // Asa evitam sa se incarce poza veche dupa un edit (ar lua-o pe cea din cache)
                string timeSignature = DateTime.Now.Ticks.ToString();

                var storagePath = Path.Combine(_env.WebRootPath, "images",
                id + timeSignature + fileExtension);

                //  Nume unic pentru fiecare task
                //  Daca pastrez numele fisierului original exista probleme
                //  atunci cand vreau sa folosesc o poza diferita dar cu acelasi nume
                //  => se va folosi prima poza cu acelasi nume
                var databaseFileName = "/images/" + id + timeSignature + fileExtension;

                // Salvare fisier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Media.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(task.Media));
                task.Media = databaseFileName;
            }

            if (TryValidateModel(task))
            {
                db.Tasks.Add(task);
                await db.SaveChangesAsync();

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

            if (task.Media != null)
            {
                var storagePath = Path.Combine(_env.WebRootPath + task.Media);

                System.IO.File.Delete(storagePath);
            }

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

            return View(task);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Edit(string id, [FromForm] Task requestTask, IFormFile Media)
        {
            Task task = db.Tasks.Find(id);

            if (Media != null && Media.Length > 0)
            {
                // Verificam extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(Media.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "Fisierul trebuie sa fie o imagine (jpg, jpeg, png, gif)");
                    return View(task);
                }

                // Stergem poza veche
                var storagePath = Path.Combine(_env.WebRootPath + task.Media);
                System.IO.File.Delete(storagePath);

                // Cale stocare
                string timeSignature = DateTime.Now.Ticks.ToString();

                storagePath = Path.Combine(_env.WebRootPath, "images",
                id + timeSignature + fileExtension);

                //  Nume unic pentru fiecare task
                //  Daca pastrez numele fisierului original exista probleme
                //  atunci cand vreau sa folosesc o poza diferita dar cu acelasi nume
                //  => se va folosi prima poza cu acelasi nume
                var databaseFileName = "/images/" + id + timeSignature + fileExtension;

                // Salvare fisier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Media.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(task.Media));
                task.Media = databaseFileName;
            }

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
