using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVCApp.Services;

namespace MVCApp.Controllers
{
    public class DapperController : Controller
    {
        private readonly IDapperService _repo;

        public DapperController(IDapperService repo)
        {
            _repo = repo;
        }

        public ActionResult Index()
        {
            var sb = new StringBuilder("Dapper service\n");

            int usersBefore = _repo.GetUsers().Count;
            sb.Append($"Users before: {usersBefore}\n");

            var user = new User
            {
                Age = 18,
                Email = "some123@email.com",
                IsMarried = false,
                Name = "some123",
                Sex = "male"
            };

            _repo.Create(user);
            sb.Append($"Create user: {user.Id}\n");
            
            int usersAfter = _repo.GetUsers().Count;
            sb.Append($"Users after: {usersAfter}\n");

            user.Name = "Username123";
            _repo.Update(user);

            User dbUser = _repo.Get(user.Id);
            sb.Append($"Update user: {dbUser.Name}\n");

            _repo.Delete(dbUser.Id);
            sb.Append("Delete user\n");

            int usersAfterDel = _repo.GetUsers().Count;
            sb.Append($"Users after delete: {usersAfterDel}\n");

            return Content(sb.ToString());
        }

        public ActionResult Test()
        {
            bool res1 = _repo.GetReturn(true);
            bool res2 = _repo.GetScalar(true);
            List<User> res3 = _repo.PassArray(new[] {2, 3});
            List<User> res4 = _repo.GetNested();
            var res5 = _repo.GetMultiple();

            return Content($"res1: {res1}, res2: {res2}");
        }
    }
}
