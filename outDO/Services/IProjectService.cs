using Microsoft.AspNetCore.Identity;
using outDO.Data;
using outDO.Models;

namespace outDO.Services
{
    public interface IProjectService
    {
        bool isUserOrganiserProject(string projectId, string userId);
        bool isUserOrganiserBoard(string boardId, string userId);
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

            if (usersId.First() != userId)
            {
                return false;
            }

            return true;
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

            if (!usersId.Any()) //imi dadea eroare ca era gol usersID si nu puteam face First()
            {
                return false;
            }

            if (usersId.First() != userId)
            {
                return false;
            }

            return true;
        }


    }
}
