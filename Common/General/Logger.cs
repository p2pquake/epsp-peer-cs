using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.Reflection;
using System.Security;
using log4net;

namespace Client.Common.General
{
    static class Logger
    {
        /// <summary>
        /// EMERG:システムが使用不能,
        /// ALERT:直ちに対処が必要,
        /// CRIT:致命的な状態,
        /// ERR:エラー状態,
        /// WARNING:警告状態,
        /// NOTICE:通常状態だが大事な状態,
        /// INFO:通知,
        /// DEBUG,デバッグレベルの情報
        /// </summary>
        public enum LogLevel
        {
            L0_EMERG = 0,
            L1_ALERT = 1,
            L2_CRIT = 2,
            L3_ERR = 3,
            L4_WARNING = 4,
            L5_NOTICE = 5,
            L6_INFO = 6,
            L7_DEBUG = 7
        }

        [Obsolete]
        public static void Write(string writeData, LogLevel logLevel)
        {
            GetLog().Debug(writeData); // + ((int)logLevel).ToString() + "<>"
        }

        [DynamicSecurityMethod]
        public static ILog GetLog()
        {
            const int callerFrameIndex = 1;
            StackFrame callerFrame = new StackFrame(callerFrameIndex);
            MethodBase callerMethod = callerFrame.GetMethod();
            return LogManager.GetLogger (Assembly.GetCallingAssembly(), callerMethod.DeclaringType);
        }
    }
}

namespace System.Security
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    internal sealed class DynamicSecurityMethodAttribute : Attribute
    {
    }
}
