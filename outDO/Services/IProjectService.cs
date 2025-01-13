using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using outDO.Data;
using outDO.Models;
using System.Drawing;
using Task = outDO.Models.Task;

namespace outDO.Services
{
    public interface IProjectService
    {
        bool isUserOrganiserProject(string projectId, string userId);
        bool isUserOrganiserBoard(string boardId, string userId);

        bool isUserOrganiserTask(string taskId, string userId);
        bool isUserOrganiserComment(string commentId, string userId);
        bool isUsersComment(string commentId, string userId);

        bool isUserTaskMember(string taskId, string userId);

        bool isUserMemberProject(string projectId, string userId);

		int deleteTask(string taskId, string userId, bool isAdmin, IWebHostEnvironment _env);
    }

    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext db;

		public ProjectService(ApplicationDbContext context)
        {
            db = context;
            
        }

        public bool isUserOrganiserProject(string projectId, string userId)
        {
            var usersId = from p in db.Projects
                         join pm in db.ProjectMembers on
                         p.Id equals pm.ProjectId
                         where p.Id == projectId
                         where pm.ProjectRole == "Organizator"
                         select pm.UserId;

            if (!usersId.Any()) //imi dadea eroare ca era gol usersID si nu puteam face First()
            {
                return false;
            }

            if (usersId.Contains(userId))
            {
                return true;
            }

            return false;

        }

        public bool isUserOrganiserBoard(string boardId, string userId)
        {
            var usersId = from b in db.Boards
                          join p in db.Projects on
                          b.ProjectId equals p.Id
                          join pm in db.ProjectMembers on
                          p.Id equals pm.ProjectId
                          where b.Id == boardId
                          where pm.ProjectRole == "Organizator"
                          select pm.UserId;

            if (!usersId.Any())
            {
                return false;
            }

            if (usersId.Contains(userId))
            {
                return true;
            }

            return false;
        }

        public bool isUserOrganiserTask(string taskId, string userId)
        {
            var usersId = from t in db.Tasks
                          join b in db.Boards on
                          t.BoardId equals b.Id
                          join p in db.Projects on
                          b.ProjectId equals p.Id
                          join pm in db.ProjectMembers on
                          p.Id equals pm.ProjectId
                          where t.Id == taskId
                          where pm.ProjectRole == "Organizator"
                          select pm.UserId;
            if (!usersId.Any())
            {
                return false;
            }
            if (usersId.Contains(userId))
            {
                return true;
            }
            return false;
        }

        public bool isUserOrganiserComment(string commentId, string userId)
        {
            var usersId = from c in db.Comments
                          join t in db.Tasks
                            on c.TaskId equals t.Id
                          join b in db.Boards on
                          t.BoardId equals b.Id
                          join p in db.Projects on
                          b.ProjectId equals p.Id
                          join pm in db.ProjectMembers on
                          p.Id equals pm.ProjectId
                          where c.Id == commentId
                          where pm.ProjectRole == "Organizator"
                          select pm.UserId;
            if (!usersId.Any())
            {
                return false;
            }
            if (usersId.Contains(userId))
            {
                return true;
            }
            return false;
        }

        public bool isUsersComment(string commentId, string userId)
        {
            var users = from c in db.Comments
                          where c.Id == commentId
                          select c.UserId;
            if (users.First() != userId)
            {
                return false; //ca sigur e doar unul
            }
            return true;
        }

        public bool isUserTaskMember(string taskId, string userId)
        {
            var usersId = from t in db.Tasks
                          join b in db.Boards on
                          t.BoardId equals b.Id
                          join p in db.Projects on
                          b.ProjectId equals p.Id
                          join pm in db.ProjectMembers on
                          p.Id equals pm.ProjectId
                          where t.Id == taskId
                          where pm.ProjectRole == "Member"
                          select pm.UserId;

            if (!usersId.Any())
            {
                return false;
            }
            if (usersId.Contains(userId))
            {
                return true;
            }
            return false;
        }

		public bool isUserMemberProject(string projectId, string userId)
		{
			var usersId = from pm in db.ProjectMembers
						  where pm.ProjectRole == "Member"
                          where pm.ProjectId == projectId
						  select pm.UserId;
			if (!usersId.Any())
			{
				return false;
			}
			if (usersId.Contains(userId))
			{
				return true;
			}
			return false;
		}

		public int deleteTask(string taskId, string userId, bool isAdmin, IWebHostEnvironment _env)
        {

			var projectId = from t in db.Tasks
							join b in db.Boards on
							t.BoardId equals b.Id
                            where taskId == t.Id
							select b.ProjectId;


			if (!isAdmin)
			{
				var isAuthorized = isUserOrganiserProject(projectId.First(), userId);
				if (!isAuthorized)
				{
					return 403;
				}
			}

			Task task = db.Tasks.Find(taskId);

			if (task.Media != null)
			{
				var storagePath = Path.Combine(_env.WebRootPath + task.Media);

				System.IO.File.Delete(storagePath);
			}

			foreach (var comment in db.Comments.Where(c => c.TaskId == taskId).ToList())
			{
				db.Comments.Remove(comment);
			}

			string boardId = task.BoardId;
			db.Tasks.Remove(task);
			db.SaveChanges();

            return 0;
		}

	}
}
