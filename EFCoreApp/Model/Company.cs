using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreApp.Model
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual List<User> Users { get; set; }
    }
}
