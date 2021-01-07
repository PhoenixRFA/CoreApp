using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreApp.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Sex { get; set; }
        public bool IsMarried { get; set; }

        public virtual UserData UserData { get; set; }
        public virtual Company Company { get; set; }
        public virtual List<Language> Languages { get; set; }
    }
}
