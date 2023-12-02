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

        public event EventHandler OnNameNotified = (s, e) => { };
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
            plugin.OnNameNotified += Plugin_OnNameNotified;
            plugin.OnReadLine += Plugin_OnReadLine;
            plugin.OnClosed += Plugin_OnClosed;
            lock (pluginList)
            {
                pluginList.Add(plugin);
            }
            plugin.BeginReceive();
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

        private void Plugin_OnNameNotified(object? sender, EventArgs e)
        {
            OnNameNotified(sender, EventArgs.Empty);
        }

        private void Plugin_OnReadLine(object? sender, PluginReadLineEventArgs e)
        {
            var packet = e.packet;

            if (packet.Code == "USRQ")
            {
                OnUserquakeRequest(sender, EventArgs.Empty);
            }

            if (packet.Code == "EXIT")
            {
                OnExitRequest(sender, EventArgs.Empty);
            }
        }

        private void Plugin_OnClosed(object? sender, EventArgs e)
        {
            lock (pluginList)
            {
                pluginList.Remove((Plugin)sender);
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
            var packet = new Net.Packet()
            {
                Code = "QUAK",
                Data = new string[] { epspQuakeEventArgs.RawAbstractString, epspQuakeEventArgs.RawDetailString },
            };
            Send(packet);
        }

        public void NotifyTsunami(EPSPTsunamiEventArgs epspTsunamiEventArgs)
        {
            var packet = new Net.Packet()
            {
                Code = "TIDL",
                Data = new string[] { epspTsunamiEventArgs.RawDetailString },
            };
            Send(packet);
        }

        public void NotifyEEW(EPSPEEWEventArgs epspEEWEventArgs)
        {
            var packet = new Net.Packet()
            {
                Code = "EEW1",
                Data = new string[] { epspEEWEventArgs.IsTest ? "1" : "0" },
            };
            Send(packet);
        }

        private void Send(Net.Packet packet)
        {
            var copyList = new List<Plugin>();
            lock (pluginList)
            {
                copyList.AddRange(pluginList);
            }

            foreach (var plugin in copyList)
            {
                plugin.Send(packet);
            }
        }
    }
}
