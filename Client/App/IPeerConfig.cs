using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.App
{
    public interface IPeerConfig
    {
        int AreaCode { get; set; }

        bool IsPortOpen { get; set; }

        int Port { get; set; }

        int MaxConnections { get; set; }
    }
}
