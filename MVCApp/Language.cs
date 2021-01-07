using System;
using System.Collections.Generic;

#nullable disable

namespace MVCApp
{
    public partial class Language
    {
        public Language()
        {
            LanguageUsers = new HashSet<LanguageUser>();
        }

        public int Id { get; set; }
        public int Name { get; set; }

        public virtual ICollection<LanguageUser> LanguageUsers { get; set; }
    }
}
