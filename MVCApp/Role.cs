using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCApp
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<User> Users { get; set; } = new List<User>();
    }
}
