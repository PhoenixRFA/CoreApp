using System;

namespace MVCApp.Services
{
    public class ApplicationService : IApplicationService
    {
        public DateTime Started { get; }
        public string Name { get; }
        public string ServerName { get; }

        public ApplicationService()
        {
            Started = DateTime.Now;
            ServerName = Environment.MachineName;
            Name = "unnamed";
        }

        public ApplicationService(string name) : this()
        {
            Name = name;
        }
    }

    public interface IApplicationService
    {
        public DateTime Started { get; }
        public string Name { get; }
        public string ServerName { get; }
    }
}
