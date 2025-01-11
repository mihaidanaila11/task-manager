using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using outDO.Data;
using outDO.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using System.Net.NetworkInformation;
using Task = outDO.Models.Task;
using System.Threading.Tasks;

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
        public async Task<IActionResult> New([FromForm] Task task, IFormFile? Media)
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
            if (userId.ToList().Contains(userManager.GetUserId(User))) 
            {
                return true;
            }

            return false;
        }

        [Authorize]
        public IActionResult Delete(string id)
        {
            if(!isUserAuthorized(id) && !User.IsInRole("Admin"))
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

            //membrii proiectului care nu sunt deja assigned la task
            var ProjectMembers = (from t in db.Tasks
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

            ViewBag.ProjectMembers = ProjectMembers;

            //membrii taskului
            var TaskMembers = (from t in db.Tasks
                                  join b in db.Boards on
                                  t.BoardId equals b.Id
                                  join pm in db.ProjectMembers on
                                    b.ProjectId equals pm.ProjectId
                                  join u in db.Users on
                                  pm.UserId equals u.Id
                                  where t.Id == id
                                  where t.TaskMembers.Contains(pm.User)
                                  select new
                                  { u.Id, u.UserName, u.Email }).ToList();

            ViewBag.TaskMembers = TaskMembers;


            return View(task);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Edit(string id, [FromForm] Task requestTask, IFormFile? Media)
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
                requestTask.Media = databaseFileName;
            }

            if (TryValidateModel(requestTask))
            {

                try
                {
                    task.Title = requestTask.Title;
                    task.Description = requestTask.Description;
                    task.Status = requestTask.Status;
                    task.DateStart = requestTask.DateStart;
                    task.DateFinish = requestTask.DateFinish;
                    task.Media = requestTask.Media;
                    task.Video = requestTask.Video;
                    
                    await db.SaveChangesAsync();
                    return Redirect("/Task/Show/" + id);
                }
                catch (Exception)
                {
                    ViewBag.Project = task;

                ViewBag.Task = task;


                    return View();
                }
                
            }

            return View(task);

        }

        [HttpGet]
        public IActionResult Show(string Id)
        {
            var task = db.Tasks.Where(t => t.Id == Id).First();
            var comments = db.Comments.Where(c => c.TaskId == task.Id).ToList();

            List<Tuple<string, Comment>> userComments = new List<Tuple<string, Comment>>();

            foreach (var comment in comments)
            {
                // poate sa fie si null / deleted user
                var username = (from c in db.Comments
                               join u in db.Users on
                               c.UserId equals u.Id
                               where c.Id == comment.Id
                               select u.UserName).First();

                if (username == null)
                {
                    username = "Deleted User";
                }

                userComments.Add(new Tuple<string, Comment> ( username, comment ));
            }

            ViewBag.comments = userComments;


            return View(task);
        }

        [HttpPost]
        public IActionResult Show(string Id, [FromForm] Comment comment)
        {
            

            string commentId = Guid.NewGuid().ToString();
            ModelState.Remove(nameof(comment.Id));
            ModelState.Remove(nameof(comment.TaskId));
            ModelState.Remove(nameof(comment.UserId));
            comment.Id = commentId;
            comment.TaskId = Id;
            comment.UserId = userManager.GetUserId(User).ToString();
            comment.Date = DateTime.Now;

            if(TryValidateModel(comment))
            {
                db.Comments.Add(comment);
                db.SaveChanges();
            }

            var task = db.Tasks.Where(t => t.Id == Id).First();
            var comments = db.Comments.Where(c => c.TaskId == task.Id).ToList();

            List<Tuple<string, Comment>> userComments = new List<Tuple<string, Comment>>();

            foreach (var comm in comments)
            {
                // poate sa fie si null / deleted user
                var username = (from c in db.Comments
                                join u in db.Users on
                                c.UserId equals u.Id
                                where c.Id == comm.Id
                                select u.UserName).First();

                if (username == null)
                {
                    username = "Deleted User";
                }

                userComments.Add(new Tuple<string, Comment>(username, comm));
            }

            ViewBag.comments = userComments;

            return View(task);
        }

        public IActionResult GoBack(string id)
        {   //ne intoarcem la boardul din care am venit
            Task task = db.Tasks.Find(id);
            return Redirect("/Board/Show/" + task.BoardId);
        }

        public IActionResult AddMember(string id, string userId)
        {
            Task task = db.Tasks.Find(id);
            if (task.TaskMembers == null)
            {
                task.TaskMembers = new List<User>();  // or whatever your User type is
            }
            task.TaskMembers.Add(db.Users.Find(userId));
            db.SaveChanges();
            return Redirect("/Task/Edit/" + id);
        }

        public IActionResult RemoveMember(string id, string userId)
        {
            Task task = db.Tasks.Include(t => t.TaskMembers).FirstOrDefault(t => t.Id == id);
            //ca sa incarce si TaskMembers
            task.TaskMembers.Remove(db.Users.Find(userId));
            db.SaveChanges();
            return Redirect("/Task/Edit/" + id);
        }
    }
}
