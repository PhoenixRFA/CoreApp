using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;
using System.Security.Principal;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBApp.Models.db
{
    public class User
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [Required]
        [BsonElement("name")]
        public string Name { get; set; }
        [Required, EmailAddress]
        [BsonElement("email")]
        public string Email { get; set; }
        [Required, Range(1, 100)]
        [BsonElement("age")]
        public int Age { get; set; }
        [BsonElement("isMarried")]
        public bool IsMarried { get; set; }
        [BsonElement("roles")]
        public string[] Roles { get; set; }
        [BsonElement("department")]
        public object Department { get; set; }
    }
}
