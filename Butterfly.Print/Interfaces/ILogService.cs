namespace Butterfly.Print.Interfaces
{
    using System;

    public interface ILogService
    {
        void Fatal(object message, Exception exception = null);

        void Error(object message, Exception exception = null);

        void Warn(object message, Exception exception = null);

        void Debug(object message, Exception exception = null);

        void Info(object message, Exception exception = null);

        void SetUserContext(Guid userId, Guid tenantId);
    }
}
