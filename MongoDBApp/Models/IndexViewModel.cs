using System.Collections.Generic;
using MongoDBApp.Models.db;

namespace MongoDBApp.Models
{
    public class IndexViewModel
    {
        public string Name { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}
