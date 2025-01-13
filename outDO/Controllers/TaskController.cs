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
using System.Net;
using outDO.Services;

namespace outDO.Controllers
{
    public class TaskController : Controller
    {
        //pas 10 user si roluri


        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly HttpClient client;
        public TaskController(ApplicationDbContext context, UserManager<User> _userManager, 
            RoleManager<IdentityRole> _roleManager, IWebHostEnvironment env)
        {
            db = context;
            userManager = _userManager;
            roleManager = _roleManager;
            _env = env;

            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            };

            client = new HttpClient();
        }

        [Authorize]
        public IActionResult New(string id)
        {
			ProjectService projectService = new ProjectService(db);

			var projectId = from t in db.Tasks
							join b in db.Boards on
							t.BoardId equals b.Id
							where t.Id == id
							select b.ProjectId;


			if (!User.IsInRole("Admin"))
			{
				var isAuthorized = projectService.isUserOrganiserProject(projectId.First(), userManager.GetUserId(User));
				if (!isAuthorized)
				{
					return Redirect("/Identity/Account/AccessDenied");
				}
			}

			ViewBag.BoardId = id;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> New([FromForm] Task task, IFormFile? Media)
        {
            string id = Guid.NewGuid().ToString();
            task.Id = id;


            // Taskul trebuie sa contina cel putin un element media
            bool mediaCheck = false;

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

                mediaCheck = true;
            }

            if (!mediaCheck)
            {
                if(task.Video == null)
                {
                    ModelState.AddModelError(string.Empty, "At least one media element");

                    ViewBag.BoardId = task.BoardId;
                    return View(task);
                }
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
            var userIds = from t in db.Tasks
                         join b in db.Boards on
                         t.BoardId equals b.Id
                         join p in db.Projects on
                         b.ProjectId equals p.Id
                         join pm in db.ProjectMembers on
                         p.Id equals pm.ProjectId
                         where t.Id == taskId
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

        [Authorize]
        public IActionResult Delete(string id)
        {
			ProjectService projectService = new ProjectService(db);

			var projectId = from t in db.Tasks
							join b in db.Boards on
							t.BoardId equals b.Id
							select b.ProjectId;


			if (!User.IsInRole("Admin"))
			{
				var isAuthorized = projectService.isUserOrganiserProject(projectId.First(), userManager.GetUserId(User));
				if (!isAuthorized)
				{
					return Redirect("/Identity/Account/AccessDenied");
				}
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
			ProjectService projectService = new ProjectService(db);

			var projectId = from t in db.Tasks
							join b in db.Boards on
							t.BoardId equals b.Id
							select b.ProjectId;

			if (!User.IsInRole("Admin"))
			{
				var isAuthorized = projectService.isUserOrganiserProject(projectId.First(), userManager.GetUserId(User));
				if (!isAuthorized)
				{
					isAuthorized = projectService.isUserTaskMember(id, userManager.GetUserId(User));
					if (!isAuthorized)
					{
						return Redirect("/Identity/Account/AccessDenied");
					}
				}
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

            bool mediaCheck = false;

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
				if (task.Media != null)
                {
					var oldStoragePath = Path.Combine(_env.WebRootPath + task.Media);
					System.IO.File.Delete(oldStoragePath);
				}
                

                // Cale stocare
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
                requestTask.Media = databaseFileName;

                mediaCheck = true;
            }

            if (!mediaCheck)
            {
                if (requestTask.Video == null)
                {
                    ModelState.AddModelError(string.Empty, "At least one media element");

                    return View(requestTask);
                }
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
                    return View(task);
                }
                
            }

            return View(task);

        }

        [HttpGet]
        public async Task<IActionResult> Show(string Id)
        {
			ProjectService projectService = new ProjectService(db);
			var projectId = from t in db.Tasks
							join b in db.Boards on
							t.BoardId equals b.Id
                            where t.Id == Id
							select b.ProjectId;

			if (!User.IsInRole("Admin"))
			{
				var isAuthorized = projectService.isUserOrganiserProject(projectId.First(), userManager.GetUserId(User));
				if (!isAuthorized)
				{
					isAuthorized = projectService.isUserTaskMember(Id, userManager.GetUserId(User));
					if (!isAuthorized)
					{
						return Redirect("/Identity/Account/AccessDenied");
					}
				}
			}
			var task = db.Tasks.Where(t => t.Id == Id).First();
            var comments = db.Comments.Where(c => c.TaskId == task.Id).ToList();

            

            // ---
            if (task.Video != null)
            {
                Tuple<string, string> videoEmbLink = new Tuple<string, string>(string.Empty, string.Empty);
                Uri videoUri = new Uri(task.Video);

                string[] YouTubeHosts = {
                        "www.youtube.com",
                        "youtube.com",
                        "youtu.be"};

                if (YouTubeHosts.Contains(videoUri.Host.ToLower()))
                {
                    string youtubeVideoId = System.Web.HttpUtility.ParseQueryString(videoUri.Query).Get("v");

                    string youtubeVideoEmbeded = "https://www.youtube.com/embed/" + youtubeVideoId + "?autoplay=0";


                    videoEmbLink = new Tuple<string,string>("youtube", youtubeVideoEmbeded);
                }

                else if (videoUri.Host.ToLower() == "www.tiktok.com")
                {
                    //Tiktok

                    string requestUrl = "https://www.tiktok.com/oembed?url=" + task.Video;

                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(requestUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();

                            videoEmbLink = new Tuple<string, string>("tiktok", responseBody);
                        }
                        else
                        {
                            //EROARE
                            // AR TREBUI SA VERIFIC IN MODEL SA EXISTE CLIPURILE!!!!!!!
                        }
                    }
                    catch (Exception ex)
                    {
                        //EROARE
                    }
                   
                }
                ViewBag.VideoEmbLinks = videoEmbLink;
            }
            // ---

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

            var tasksM = db.Tasks
            .Include(t => t.TaskMembers)
            .Where(t => t.Id == Id)
            .ToList();
            ViewBag.Tasks = tasksM;


            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> Show(string Id, [FromForm] Comment comment)
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

				return Redirect("/Task/Show/" + Id);
			}
            else
            {
				var task = db.Tasks.Where(t => t.Id == Id).First();
				var comments = db.Comments.Where(c => c.TaskId == task.Id).ToList();

				// ---
				if (task.Video != null)
				{
					Tuple<string, string> videoEmbLink = new Tuple<string, string>(string.Empty, string.Empty);
					Uri videoUri = new Uri(task.Video);

					string[] YouTubeHosts = {
						"www.youtube.com",
						"youtube.com",
						"youtu.be"};

					if (YouTubeHosts.Contains(videoUri.Host.ToLower()))
					{
						string youtubeVideoId = System.Web.HttpUtility.ParseQueryString(videoUri.Query).Get("v");

						string youtubeVideoEmbeded = "https://www.youtube.com/embed/" + youtubeVideoId + "?autoplay=0";


						videoEmbLink = new Tuple<string, string>("youtube", youtubeVideoEmbeded);
					}

					else if (videoUri.Host.ToLower() == "www.tiktok.com")
					{
						//Tiktok

						string requestUrl = "https://www.tiktok.com/oembed?url=" + task.Video;

						try
						{
							HttpResponseMessage response = await client.GetAsync(requestUrl);

							if (response.IsSuccessStatusCode)
							{
								string responseBody = await response.Content.ReadAsStringAsync();

								videoEmbLink = new Tuple<string, string>("tiktok", responseBody);
							}
							else
							{
								//EROARE
								// AR TREBUI SA VERIFIC IN MODEL SA EXISTE CLIPURILE!!!!!!!
							}
						}
						catch (Exception ex)
						{
							//EROARE
						}

					}
					ViewBag.VideoEmbLinks = videoEmbLink;
				}
				// ---


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

            // 

            

            
            
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
