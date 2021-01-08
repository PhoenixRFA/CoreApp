using System;
using System.Collections.Generic;

#nullable disable

namespace MVCApp
{
    public partial class User
    {
        public User()
        {
            LanguageUsers = new HashSet<LanguageUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Sex { get; set; }
        public bool IsMarried { get; set; }
        public int? UserDataId { get; set; }
        public int? CompanyId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public virtual Company Company { get; set; }
        public virtual UserDatum UserData { get; set; }
        public virtual ICollection<LanguageUser> LanguageUsers { get; set; }
    }
}
