using Serilog;
using Serilog.Core;
using System;

namespace tSync
{
    public static class Log
    {
        private static readonly Logger logger;

        static Log()
        {
            logger = new LoggerConfiguration().WriteTo.File("consoleapp.log").WriteTo.Console().CreateLogger();
        }

        public static void Exception(Exception ex, string message = null)
        {
            logger.Error(ex, $"Exception {message}");
        }

        public static void Message(string message)
        {
            logger.Information(message);
        }
    }
}
