using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebAPIApp
{
    public class UsersStore : IUserStore
    {
        private static readonly List<UserModel> Db = new List<UserModel>();

        public List<UserModel> Get()
        {
            return Db.ToList();
        }

        public int Save(UserModel model)
        {
            if (model.Id == 0)
            {
                int newID = Db.DefaultIfEmpty(new UserModel()).Max(x => x.Id) + 1;

                model.Id = newID;

                Db.Add(model);

                return newID;
            }
            else
            {
                UserModel dbUser = Db.FirstOrDefault(x => x.Id == model.Id);
                if (dbUser == null) return 0;

                dbUser.Age = model.Age;
                dbUser.Name = model.Name;

                return dbUser.Id;
            }
        }

        public void Delete(int id)
        {
            if (id <= 0) return;

            Db.RemoveAll(x => x.Id == id);
        }
    }

    public class UserModel
    {
        [Range(0, int.MaxValue, ErrorMessage = "Id must be positive")]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required, Range(1, 120)]
        public int Age { get; set; }
    }

    public interface IUserStore
    {
        List<UserModel> Get();
        int Save(UserModel model);
        void Delete(int id);
    }
}
