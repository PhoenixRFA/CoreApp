using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreApp.Services.Loggers.BaseLogger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreApp.Services.Loggers.FileLogger
{
    //��������� ���������� ��� ��������� ������������. �������� ���������� �������� ���� �� �������� ������ (FileLoggerProvider), ���� ���������� (File)
    [ProviderAlias("File")]
    public class FileLoggerProvider : LoggerProvider
    {
        private bool _terminated;
        private int _counter;
        private string _filePath;
        private readonly Dictionary<string, int> _lengths = new Dictionary<string, int>();
        internal FileLoggerConfiguration Config;

        private readonly ConcurrentQueue<LogEntry> _queue = new ConcurrentQueue<LogEntry>();

        #region System

        /// <summary> �������� ������, ���� ��������� ��������� �� ������� <paramref name="maxLength"/> </summary>
        /// <param name="text"></param>
        /// <param name="maxLength"></param>
        private string Pad(string text, int maxLength)
        {
            //PadRight - ��������� ������ ��������� �� ������� �������
            if (string.IsNullOrWhiteSpace(text)) return string.Empty.PadRight(maxLength);

            if (text.Length > maxLength) return text.Substring(0, maxLength);

            return text.PadRight(maxLength);
        }

        /// <summary> ���������� ������� �������� "�������" </summary>
        private void PrepareLengths()
        {
            _lengths["Time"]     = 24;
            _lengths["Host"]     = 16;
            _lengths["User"]     = 16;
            _lengths["Level"]    = 14;
            _lengths["EventId"]  = 32;
            _lengths["Category"] = 92;
            _lengths["Scope"]    = 64;
        }

        #endregion

        public FileLoggerProvider(IOptionsMonitor<FileLoggerConfiguration> monitor) : this(monitor.CurrentValue)
        {
            SettingsChangeToken = monitor.OnChange(cfg =>
            {
                Config = cfg;
            });
        }

        public FileLoggerProvider(FileLoggerConfiguration config)
        {
            PrepareLengths();
            Config = config;
            
            //�������� ������� �����
            BeginFile();

            //������ �������� ������
            ThreadProc();
        }

        //���������� �������� ���������
        private void ApplyRetainPolicy()
        {
            try
            {
                //������� �� ����� � ������ ��� ����� ����� � �������� �� ���� ��������
                List<FileInfo> fileList = new DirectoryInfo(Config.LogsFolderPath)
                    .GetFiles("*.log", SearchOption.TopDirectoryOnly)
                    .OrderBy(x => x.CreationTime)
                    .ToList();

                //������ ����, ���� �� ���������� ��������� �������� ���������
                while (fileList.Count >= Config.RetainPolicyFileCount)
                {
                    FileInfo file = fileList.First();
                    file.Delete();
                    fileList.Remove(file);
                }
            }
            catch { /*ignore*/ }
        }

        /// <summary> ��������� ������ ����� � ���������� � ���� ������, ���� ������� ����� ���� � ����� ��� � ���� </summary>
        private void WriteLine(string text)
        {
            _counter++;

            //�������� ������� ����� ������ 100 �������
            if (_counter % 100 == 0)
            {
                var file = new FileInfo(_filePath);
                if (file.Length > 1024 * Config.MaxFileSizeKb)
                {
                    BeginFile();
                }
            }

            File.AppendAllText(_filePath, text);
        }

        /// <summary> ������������� ����� ��� ������ (�������� �����, ������������ ����������) </summary>
        private void BeginFile()
        {
            Directory.CreateDirectory(Config.LogsFolderPath);
            string logFileName = $"{LogEntry.StaticHostName}-{DateTime.Now:yyyyMMdd-HHmm}.log";

            _filePath = Path.Combine(Config.LogsFolderPath, logFileName);

            var sb = new StringBuilder();
            AddTitle("Time");
            AddTitle("Host");
            AddTitle("User");
            AddTitle("Level");
            AddTitle("EventId");
            AddTitle("Category");
            AddTitle("Scope");
            sb.AppendLine("Text");

            File.WriteAllText(_filePath, sb.ToString());

            ApplyRetainPolicy();


            void AddTitle(string name) => sb.Append(Pad(name, _lengths[name]));
        }

        /// <summary> ������������ ������ �� �������� ���� � ������ � ���� </summary>
        private void WriteLogFile()
        {
            if (!_queue.TryDequeue(out LogEntry log)) return;

            var sb = new StringBuilder();
            sb.Append(Pad(log.TimeStampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.ff"), _lengths["Time"]));
            sb.Append(Pad(log.HostName, _lengths["Host"]));
            sb.Append(Pad(log.UserName, _lengths["User"]));
            sb.Append(Pad(log.Level.ToString(), _lengths["Level"]));
            sb.Append(Pad(log.EventId.ToString(), _lengths["EventId"]));
            sb.Append(Pad(log.Category, _lengths["Category"]));

            string s = string.Empty;
            if (log.Scopes != null && log.Scopes.Count > 0)
            {
                LogScopeInfo scopeInfo = log.Scopes.Last();
                if (!string.IsNullOrWhiteSpace(scopeInfo.Text))
                {
                    s = scopeInfo.Text;
                }
            }

            sb.Append(Pad(s, _lengths["Scope"]));

            string text = log.Text;
                
            //������ ���������� �������
            //if (log.StateProperties != null && log.StateProperties.Count > 0)
            //{
            //    text = text + " Properties = " + JsonConvert.SerializeObject(log.StateProperties);
            //}

            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.Append(text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " "));
            }

            sb.AppendLine();

            WriteLine(sb.ToString());
        }

        /// <summary> ������� ������ ����� </summary>
        private void ThreadProc()
        {
            //���������� � BackgroundThread ��. LogProcessor
            Task.Run(async () =>
            {
                while (!_terminated)
                {
                    try
                    {
                        WriteLogFile();
                        await Task.Delay(100);
                        //Thread.Sleep(100);
                    }
                    catch { /*ignore*/ }
                }
            });
        }

        /// <summary> ��� ���������� ������ ����������� ������� ������ </summary>
        protected override void Dispose(bool disposing)
        {
            //TODO ������������ CancelationToken
            _terminated = true;
            base.Dispose(disposing);
        }

        /// <summary> �������� �� ���������? </summary>
        public override bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && Config.LogLevel != LogLevel.None && logLevel >= Config.LogLevel;
        }

        /// <summary> �������� ��� � ������� �� ������ </summary>
        public override void WriteLog(LogEntry logEntry)
        {
            _queue.Enqueue(logEntry);
        }
    }
}