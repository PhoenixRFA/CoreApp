using System.Collections.Generic;

namespace CoreApp.Services.Loggers.BaseLogger
{
    /// <summary> Представляет собой информацию об области логгера </summary>
    public class LogScopeInfo
    {
        public string Text { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
