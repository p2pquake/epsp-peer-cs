using Client.Common.Net;
using Client.Peer;

using LegacyPluginSupporter.Net;

using System.Net.Sockets;

namespace LegacyPluginSupporter
{
    class Manager
    {
        private const int PORT = 6918;

        private List<Plugin> pluginList = new();
        private AsyncListener asyncListener;
        private string serverName;

        public IReadOnlyList<Plugin> PluginList { get { return pluginList; } }

        public event EventHandler OnUserquakeRequest = (s, e) => { };
        public event EventHandler OnExitRequest = (s, e) => { };

        public Manager(string serverName)
        {
            this.serverName = serverName;
        }

        public bool Listen()
        {
            Shutdown();

            try
            {
                asyncListener = new AsyncListener(PORT);
                asyncListener.Accept += AsyncListener_Accept;
                asyncListener.Start();
            }
            catch (SocketException)
            {
                asyncListener = null;
                return false;
            }

            return true;
        }

        private void AsyncListener_Accept(object? sender, AcceptEventArgs e)
        {
            var plugin = new Plugin(new CRLFSocket(e.Socket), serverName);
            plugin.OnReadLine += Plugin_OnReadLine;
            plugin.OnClosed += Plugin_OnClosed;
            lock (pluginList)
            {
                pluginList.Add(plugin);
            }
            plugin.BeginReceive();
        }

        private void Plugin_OnClosed(object? sender, EventArgs e)
        {
            lock (pluginList)
            {
                pluginList.Remove((Plugin)sender);
            }
        }

        public bool Shutdown()
        {
            if (asyncListener == null)
            {
                return false;
            }

            asyncListener.Stop();
            return true;
        }

        private void Plugin_OnReadLine(object? sender, PluginReadLineEventArgs e)
        {
            var packet = e.packet;

            if (packet.Code == "USRQ")
            {
                OnUserquakeRequest(this, EventArgs.Empty);
            }

            if (packet.Code == "EXIT")
            {
                OnExitRequest(this, EventArgs.Empty);
            }
        }

        public void RequestOption(Plugin plugin)
        {
            var packet = new Net.Packet()
            {
                Code = "OPTI",
            };

            lock (pluginList)
            {
                if (pluginList.Contains(plugin))
                {
                    plugin.Send(packet);
                }
            }
        }

        public void NotifyEarthquake(EPSPQuakeEventArgs epspQuakeEventArgs)
        {
            throw new NotImplementedException();
        }

        public void NotifyTsunami(EPSPTsunamiEventArgs epspTsunamiEventArgs)
        {
            throw new NotImplementedException();
        }

        public void NotifyEEW(EPSPEEWEventArgs epspEEWEventArgs)
        {
            var packet = new Net.Packet()
            {
                Code = "EEW1",
                Data = new string[] { epspEEWEventArgs.IsTest ? "1" : "0" },
            };
        }
    }
}
