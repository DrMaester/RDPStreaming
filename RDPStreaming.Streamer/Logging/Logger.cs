using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using log4net;

namespace RDPStreaming.Streamer.Logging
{
    public static class Logger
    {
        public static ILog Get([CallerMemberName] string name = "")
        {
            return log4net.LogManager.GetLogger(Assembly.GetExecutingAssembly(), name);
        }
    }
}
