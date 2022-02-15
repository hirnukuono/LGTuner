using BepInEx.Logging;

namespace LGTuner
{
    internal static class Logger
    {
        private static readonly ManualLogSource _logger;

        static Logger()
        {
            // We create our private logger instance with the plugin name LGTuner
            _logger = new ManualLogSource("LGTuner");
            // We add the private logger instance to BepInEx's logger sources
            // so we will be able to output to the console
            BepInEx.Logging.Logger.Sources.Add(_logger);
        }

        // Helper method for formatting messages, currently converts the provided
        // 'msg' object to a string, but it can be extended however you want
        private static string Format(object msg) => msg.ToString();

        // Helper methods for logging

        public static void Info(object data) => _logger.LogMessage(Format(data));
        public static void Verbose(object data)
        {
            // This block of code will only compile if the user is compiling in Debug mode
            // so you can have more informative logging that will be automatically removed
            // when publishing your plugin.
#if DEBUG
            _logger.LogDebug(Format(data));
#endif
        }
        public static void Debug(object data) => _logger.LogDebug(Format(data));
        public static void Error(object data) => _logger.LogError(Format(data));
    }
}
