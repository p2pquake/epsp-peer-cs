using Mono.Nat;

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
        public static bool OpenPort(int port)
        {
            bool completed = false;
            // 別 Task で走らせて 3 秒だけは待つ。
            Task.Run(() =>
            {
                NatUtility.DeviceFound += (s, e) =>
                {
                    var device = e.Device;
                    try
                    {
                        device.CreatePortMap(new Mapping(Protocol.Tcp, port, port));
                    } catch (Exception)
                    {
                        // 開放に失敗しても何もしない.
                    }
                    completed = true;
                };
                NatUtility.StartDiscovery(NatProtocol.Upnp);
                Thread.Sleep(3000);
                NatUtility.StopDiscovery();
            }).Wait();
            return completed;
        }
    }
}
