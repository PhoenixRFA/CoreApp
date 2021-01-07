using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreApp.Model
{
    public class Language
    {
        public int Id { get; set; }
        public int Name { get; set; }

        public virtual List<User> Users { get; set; }
    }
}
