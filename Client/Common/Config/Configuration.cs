using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Common.Config
{
    class Configuration
    {
        private Configuration() { }

        static Configuration Instance { get; } = new Configuration();

        bool IsListen { get; set; }
        int ListenPort { get; set; }

        int MaxConnections { get; set; }
        int ConnectionTimeoutSeconds { get; set; }

        string AreaCode { get; set; }
    }
}
