using System.Collections.Generic;

namespace MVCApp.Models
{
    public class CacheIndexModel
    {
        public List<User> Users { get; set; }
        public bool IsFromCache { get; set; }
    }
}
