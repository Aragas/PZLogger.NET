namespace PZLogger.NET.Exception
{
    public class ReportToConsoleException : System.Exception
    {
        public ReportToConsoleException() : base() { }

        public ReportToConsoleException(string message) : base(message) { }

        public ReportToConsoleException(string format, params object[] args) : base(string.Format(format, args)) { }

        public ReportToConsoleException(string message, System.Exception innerException) : base(message, innerException) { }

        public ReportToConsoleException(string format, System.Exception innerException, params object[] args) : base(string.Format(format, args), innerException) { }
    }
}