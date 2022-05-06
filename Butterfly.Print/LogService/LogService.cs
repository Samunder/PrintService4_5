namespace Butterfly.Print.LogService
{
    using System;
    using Interfaces;

    public class LogService : ILogService
    {
        /// <summary>
        /// Action is defined like this LogLevel, Message, Exception
        /// LogLevel can be: INFO, DEBUG, ERROR, FATAL, WARN
        /// </summary>
        private readonly Action<string, string, Exception> log;

        public LogService()
        {
        }

        public LogService(Action<string, string, Exception> log)
        {
            this.log = log;
        }

        public Guid UserId { get; set; }

        public Guid TenantId { get; set; }

        public void Fatal(object message, Exception exception = null)
        {
            if (this.log != null)
            {
                this.log("FATAL", message.ToString(), exception);
            }
            else
            {
                Console.WriteLine(message.ToString());
            }
        }

        public void Error(object message, Exception exception = null)
        {
            if (this.log != null)
            {
                this.log("ERROR", message.ToString(), exception);
            }
            else
            {
                Console.WriteLine(message.ToString());
            }
        }

        public void Warn(object message, Exception exception = null)
        {
            if (this.log != null)
            {
                this.log("WARN", message.ToString(), exception);
            }
            else
            {
                Console.WriteLine(message.ToString());
            }
        }

        public void Debug(object message, Exception exception = null)
        {
            if (this.log != null)
            {
                this.log("DEBUG", message.ToString(), exception);
            }
            else
            {
                Console.WriteLine(message.ToString());
            }
        }

        public void Info(object message, Exception exception = null)
        {
            if (this.log != null)
            {
                this.log("INFO", message.ToString(), exception);
            }
            else
            {
                Console.WriteLine(message.ToString());
            }
        }

        public void SetUserContext(Guid userId, Guid tenantId)
        {
            this.UserId = userId;
            this.TenantId = tenantId;
        }
    }
}
