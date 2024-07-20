using System.Runtime.CompilerServices;

namespace TaskManager.Helpers{



public static class LoggerExtensions{

public static void LogInformationWithMethod(this ILogger logger, string message, [CallerMemberName] string callerMethodName = "")
    {
        logger.LogInformation($"{callerMethodName}: {message}");
    }

    public static void LogWarningWithMethod(this ILogger logger, string message, [CallerMemberName] string callerMethodName = "")
    {
        logger.LogWarning($"{callerMethodName}: {message}");
    }

    public static void LogErrorWithMethod(this ILogger logger, string message, [CallerMemberName] string callerMethodName = "")
    {
        logger.LogError($"{callerMethodName}: {message}");
    }
    
}
}