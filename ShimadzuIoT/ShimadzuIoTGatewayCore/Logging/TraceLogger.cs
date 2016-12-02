using System.Diagnostics;

namespace ShimadzuIoTGatewayCore.Logging
{
    /// <summary>
    /// Default implementation of ILogger with the System.Diagnostics.Trace 
    /// object as the logger.
    /// </summary>
    public class TraceLogger : ILogger
    {
        public void LogInfo(string message)
        {
#if !WINDOWS_UWP
            Trace.TraceInformation(message);
#endif
        }

        public void LogInfo(string format, params object[] args)
        {
#if !WINDOWS_UWP
            Trace.TraceInformation(format, args);
#endif
        }

        public void LogWarning(string message)
        {
#if !WINDOWS_UWP
            Trace.TraceWarning(message);
#endif
        }

        public void LogWarning(string format, params object[] args)
        {
#if !WINDOWS_UWP
            Trace.TraceWarning(format, args);
#endif
        }

        public void LogError(string message)
        {
#if !WINDOWS_UWP
            Trace.TraceError(message);
#endif
        }

        public void LogError(string format, params object[] args)
        {
#if !WINDOWS_UWP
            Trace.TraceError(format, args);
#endif
        }
    }
}
