namespace Core.Interfaces
{
    public interface ILog
    {
        void Debug(string message);

        void Information(string message);

        void Warning(string message);

        void Error(string message);
    }
}
