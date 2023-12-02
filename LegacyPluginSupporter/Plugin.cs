using LegacyPluginSupporter.Net;

namespace LegacyPluginSupporter
{
    class PluginReadLineEventArgs : EventArgs
    {
        public Packet packet;
    }

    internal class Plugin
    {
        public string PluginName { get; private set; } = "";
        public DateTime ConnectedAt { get; init; }

        private readonly CRLFSocket socket;
        private readonly string serverName;

        public event EventHandler OnClosed = (s, e) => { };
        public event EventHandler OnNameNotified = (s, e) => { };
        public event EventHandler<PluginReadLineEventArgs> OnReadLine = (s, e) => { };

        public Plugin(CRLFSocket socket, string serverName)
        {
            this.socket = socket;
            this.serverName = serverName;
            socket.Closed += Socket_Closed;
            socket.ReadLine += Socket_ReadLine;
        }

        public void BeginReceive()
        {
            socket.BeginReceive();
        }

        public void Disconnect()
        {
            socket.Close();
        }

        public void Send(Packet packet)
        {
            socket.WriteLine(packet.ToPacketString());
        }

        private void Socket_Closed(object? sender, EventArgs e)
        {
            OnClosed(this, EventArgs.Empty);
        }

        private void Socket_ReadLine(object? sender, ReadLineEventArgs e)
        {
            if (e.packet.Code == "JOIN")
            {
                PluginName = e.packet.Data[0];
                Send(new Packet() { Code = "SVER", Data = new string[] { serverName } });
                OnNameNotified(this, EventArgs.Empty);
                return;
            }

            OnReadLine(this, new PluginReadLineEventArgs()
            {
                packet = e.packet
            });
        }
    }
}
