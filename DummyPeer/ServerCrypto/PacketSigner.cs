using Client.Common.Net;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyPeer.ServerCrypto
{
    public class PacketSigner
    {
        public static Packet Sign(Packet packet)
        {
            var expire = DateTime.Now.AddMinutes(5);
            var data = "";

            if (packet.Code == 551)
            {
                data = packet.Data[0] + packet.Data[1];
            }
            if (packet.Code == 552)
            {
                data = packet.Data[0];
            }
            if (packet.Code == 561)
            {
                data = packet.Data[0];
            }

            var signature = DataSigner.SignServerData(data, expire);

            packet.Data = new string[] { signature, expire.ToString("yyyy/MM/dd HH-mm-ss") }.Concat(packet.Data).ToArray();

            return packet;
        }

    }
}
