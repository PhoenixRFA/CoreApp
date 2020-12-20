using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;

namespace CoreApp.Services.Loggers.BaseLogger
{
    /// <summary> Представляет собой единицу лога </summary>
    public class LogEntry
    {
        public static readonly string StaticHostName = Dns.GetHostName();

        public string UserName => Environment.UserName;
        public string HostName => StaticHostName;
        public DateTime TimeStampUtc => DateTime.UtcNow;

        public string Category { get; set; }
        public LogLevel Level { get; set; }
        public string Text { get; set; }
        public Exception Exception { get; set; }
        public EventId EventId { get; set; }
        public object State { get; set; }
        public string StateText { get; set; }
        public Dictionary<string, object> StateProperties { get; set; }
        public List<LogScopeInfo> Scopes { get; set; }
    }
}
