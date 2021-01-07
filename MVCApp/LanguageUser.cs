using System;
using System.Collections.Generic;

#nullable disable

namespace MVCApp
{
    public partial class LanguageUser
    {
        public int LanguagesId { get; set; }
        public int UsersId { get; set; }

        public virtual Language Languages { get; set; }
        public virtual User Users { get; set; }
    }
}
