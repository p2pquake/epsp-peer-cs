using Open.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Common.Net.UPnP
{
    public class UPnPUtil
    {
        public static bool OpenPort(int port, string name = "EPSP Client")
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(5000);
                var deviceTask = discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
                deviceTask.Wait();
                var device = deviceTask.Result;
                var mapTask = device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, name));
                mapTask.Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerException is NatDeviceNotFoundException || e.InnerException is MappingException)
                {
                    return false;
                }
                if (e.InnerException is WebException)
                {
                    return false;
                }
                throw e;
            }

            return true;
        }
    }
}
