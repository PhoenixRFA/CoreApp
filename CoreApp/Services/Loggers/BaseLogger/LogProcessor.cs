using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CoreApp.Services.Loggers.BaseLogger
{
    /// <summary> Базовый класс для обработки логов в фоновом потоке </summary>
    public abstract class LogProcessor : IDisposable
    {
        private const int MaxQueuedMessages = 1024;

        private readonly BlockingCollection<LogEntry> _messageQueue = new BlockingCollection<LogEntry>(MaxQueuedMessages);
        private readonly Thread _backgroundThread;

        protected LogProcessor(string name)
        {
            //Вся работа происходит в фоновом потоке
            _backgroundThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = name
            };

            _backgroundThread.Start();
        }

        /// <summary> Добавление лога в очередь </summary>
        public virtual void EnqueueMessage(LogEntry log) //TODO нужен ли тут virtual
        {
            //Если очередь больше принимает данные
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    //помещаем лог в очередь
                    _messageQueue.Add(log);
                    return;
                }
                catch (InvalidOperationException) { /* ignored */ }
            }

            //Если очередь НЕ принимает данные - записываем в лог данные в текущем потоке (поток может блокироваться на время записи)
            //Очередь не принимает данные, если фоновый поток завершился (из-за ошибки) или класс уже утилизирован (Disposed)
            WriteMessage(log);
        }
        
        /// <summary> Задача рабочего потока - запись логов из очереди </summary>
        private void ProcessLogQueue()
        {
            try
            {
                foreach (LogEntry log in _messageQueue.GetConsumingEnumerable())
                {
                    WriteMessage(log);
                }
            }
            catch
            {
                try
                {
                    _messageQueue.CompleteAdding();
                }
                catch { /* ignored */ }
            }
        }

        //TODO Нужен ли тут virtual?
        public virtual void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                //блокирует текущий поток, пока backgroundThread не завершится
                _backgroundThread.Join();
            }
            catch(ThreadStateException) { /* ignored */ }
        }
        
        /// <summary> Метод записи лога </summary>
        internal abstract void WriteMessage(LogEntry log);
    }
}
