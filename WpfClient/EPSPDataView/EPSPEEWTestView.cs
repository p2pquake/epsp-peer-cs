using Client.Peer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.EPSPDataView
{
    public class EPSPEEWTestView
    {
        public EPSPEEWTestEventArgs EventArgs { get; init; }

        public string Time => EventArgs.ReceivedAt.ToString("dd日HH時mm分");

        public string DetailTitle => $"緊急地震速報 発表検出（{(EventArgs is not null ? EventArgs.ReceivedAt.ToString("dd日HH時mm分ss秒") : "")}）";
    }
}
