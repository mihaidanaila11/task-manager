using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using outDO.Data;
using outDO.Models;
using outDO.Services;
using System;
using System.Net;
using System.Threading.Tasks;


namespace outDO.Controllers
{
    public class BoardController : Controller
    {
        //pas 10 user si roluri
        

        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly HttpClient client;
		private readonly IWebHostEnvironment env;
		public BoardController(ApplicationDbContext context,UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager,
			IWebHostEnvironment _env)

		{
            db = context;
            userManager = _userManager;
            roleManager = _roleManager;
            env = _env; 

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

			if (!User.IsInRole("Admin"))
			{
				var isAuthorized = projectService.isUserOrganiserProject(id, userManager.GetUserId(User));
				if (!isAuthorized)
				{
					return Redirect("/Identity/Account/AccessDenied");
				}
			}

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
        public async Task<IActionResult> Show(string id)
        {

			ProjectService projectService = new ProjectService(db);
			Board board = db.Boards.Where(b => b.Id == id).First();

			if (!User.IsInRole("Admin"))
			{
				var isAuthorized = projectService.isUserOrganiserProject(board.ProjectId, userManager.GetUserId(User));
				if (!isAuthorized)
				{
					isAuthorized = projectService.isUserMemberProject(board.ProjectId, userManager.GetUserId(User));
					if (!isAuthorized)
					{
						return Redirect("/Identity/Account/AccessDenied");
					}
				}
			}

            var tasks = db.Tasks.Where(t => t.BoardId == board.Id).ToList();

            int perPage = 3;

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            int offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * perPage;
            }

            var paginatedTasks = tasks.Skip(offset).Take(perPage);

            ViewBag.lastPage = Math.Ceiling((float)tasks.Count() / (float)perPage);
            ViewBag.Tasks = paginatedTasks;
            ViewBag.PaginationBaseUrl ="?page";

            ViewBag.paginatedTasks = paginatedTasks;

            Dictionary<string, Tuple<string, string>> videoEmbLinks = new Dictionary<string, Tuple<string, string>>();

            foreach(var paginatedTask in paginatedTasks)
            {
                if (paginatedTask.Video != null)
                {
                    Uri videoUri = new Uri(paginatedTask.Video);

                    string[] YouTubeHosts = { 
                        "www.youtube.com",
                        "youtube.com",
                        "youtu.be"};

                    if (YouTubeHosts.Contains(videoUri.Host.ToLower()))
                    {
                        string youtubeVideoId = System.Web.HttpUtility.ParseQueryString(videoUri.Query).Get("v");

                        string youtubeVideoEmbeded = "https://www.youtube.com/embed/" + youtubeVideoId + "?autoplay=0";


                        videoEmbLinks.Add(paginatedTask.Id, new Tuple<string, string>("youtube", youtubeVideoEmbeded));
                    }

                    else if(videoUri.Host.ToLower() == "www.tiktok.com")
                    {
                        //Tiktok

                        string requestUrl = "https://www.tiktok.com/oembed?url=" + paginatedTask.Video;

                        try
                        {
                            HttpResponseMessage response = await client.GetAsync(requestUrl);

                            if (response.IsSuccessStatusCode)
                            {
                                string responseBody = await response.Content.ReadAsStringAsync();

                                videoEmbLinks.Add(paginatedTask.Id, new Tuple<string, string>("tiktok", responseBody));
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
                }
            }

            ViewBag.videoEmbLinks = videoEmbLinks;

            var tasksM = db.Tasks
            .Include(t => t.TaskMembers)
            .Where(t => t.BoardId == board.Id)
            .ToList();
            ViewBag.Tasks = tasksM;


            return View(board);
        }

        private bool isUserAuthorized(string boardId)
        {
            var userIds = from b in db.Boards
                         join p in db.Projects on
                         b.ProjectId equals p.Id
                         join pm in db.ProjectMembers on
                         p.Id equals pm.ProjectId
                         where b.Id == boardId
                         where pm.ProjectRole == "Organizator"
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
			Board board = db.Boards.Where(b => b.Id == id).First();

			if (!User.IsInRole("Admin"))
			{
				var isAuthorized = projectService.isUserOrganiserProject(board.ProjectId, userManager.GetUserId(User));
				if (!isAuthorized)
				{
					return Redirect("/Identity/Account/AccessDenied");
				}
			}

            string projectId = board.ProjectId;

            foreach (var task in db.Tasks.Where(t => t.BoardId == id))
            {
				string userId = userManager.GetUserId(User);
				User user = db.Users.Find(userId);


				projectService.deleteTask(task.Id, userId, User.IsInRole("Admin"), env);
            }

            db.Boards.Remove(board);
            db.SaveChanges();

            return Redirect("/Project/Show/" + projectId);
        }

        [Authorize]
        public IActionResult Edit(string id)
        {
			ProjectService projectService = new ProjectService(db);
			Board board = db.Boards.Where(b => b.Id == id).First();

			if (!User.IsInRole("Admin"))
			{
				var isAuthorized = projectService.isUserOrganiserProject(board.ProjectId, userManager.GetUserId(User));
				if (!isAuthorized)
				{
					return Redirect("/Identity/Account/AccessDenied");
				}
			}

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
 

        public IActionResult Test(string id)
        {
            Board board = db.Boards.Where(b => b.Id == id).First();


            var notStartedTasks = db.Tasks.Where(t => t.BoardId == board.Id && t.Status.ToLower() == "not started").ToList();
            var doingTasks = db.Tasks.Where(t => t.BoardId == board.Id && t.Status.ToLower() == "in progress").ToList();
            var finishedTasks = db.Tasks.Where(t => t.BoardId == board.Id && t.Status.ToLower() == "finished").ToList();
            
            ViewBag.notStartedTasks = notStartedTasks;
            ViewBag.doingTasks = doingTasks;
            ViewBag.finishedTasks = finishedTasks;

            return View(board);
        }
    }
}
