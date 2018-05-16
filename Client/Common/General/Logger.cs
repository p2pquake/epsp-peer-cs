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
