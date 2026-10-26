using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using LGTuner.Manager;

namespace LGTuner
{
    internal static class Logger
    {
        public static ManualLogSource MLS { get; internal set; } = null!;

        public static void Error(BepInExErrorLogInterpolatedStringHandler handler) => MLS.LogError(handler);
        public static void Error(string str) => MLS.LogError(str);
        public static void Warn(BepInExWarningLogInterpolatedStringHandler handler) => MLS.LogWarning(handler);
        public static void Warn(string str) => MLS.LogWarning(str);
        public static void Message(BepInExMessageLogInterpolatedStringHandler handler) => MLS.LogMessage(handler);
        public static void Message(string str) => MLS.LogMessage(str);
        public static void Info(BepInExInfoLogInterpolatedStringHandler handler) => MLS.LogInfo(handler);
        public static void Info(string str) => MLS.LogMessage(str);
        public static void Debug(BepInExDebugLogInterpolatedStringHandler handler) => MLS.LogDebug(handler);
        public static void Debug(string str) => MLS.LogDebug(str);
        
        public static void Verbose(string str, LogLevel level = LogLevel.Debug)
        {
            if (ConfigManager.General.VerboseEnabled.Value)
            {
                switch (level)
                {
                    case LogLevel.Error:
                        Error(str);
                        return;
                    case LogLevel.Warning:
                        Warn(str);
                        return;
                    case LogLevel.Message:
                        Message(str);
                        return;
                    case LogLevel.Info:
                        Info(str);
                        return;
                    case LogLevel.Debug:
                        Debug(str);
                        return;
                }
            }
        }
    }
}
