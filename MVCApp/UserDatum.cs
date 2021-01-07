using System;
using System.Collections.Generic;

#nullable disable

namespace MVCApp
{
    public partial class UserDatum
    {
        public UserDatum()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Additional { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
