using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Common.Net
{
    class Packet
    {
        private int code;
        private int hop;
        private string[] data;

        public int Code {
            get { return code; }
            set { code = value; }
        }

        public int Hop {
            get { return hop; }
            set { hop = value; }
        }

        public string[] Data {
            get { return data; }
            set { data = value; }
        }

        public static Packet Parse(string line)
        {
            Packet packet = new Packet();
            if (packet.parse(line))
            {
                return packet;
            }
            else
            {
                throw new FormatException();
            }
        }

        private bool parse(string packet)
        {
            // 改行は削除
            packet = packet.Replace("\r", "").Replace("\n", "");
            string[] datas = packet.Split(new char[] { ' ' }, 3);

            // 初期値
            code = -1;
            hop = -1;
            data = null;

            if (datas.Length <= 0 || datas.Length >= 4)
            {
                return false;
            }
            else
            {
                if (!int.TryParse(datas[0], out code))
                    return false;

                if (datas.Length >= 2 && !int.TryParse(datas[1], out hop))
                    return false;

                if (datas.Length == 3)
                    data = datas[2].Split(new char[] { ':' });

                return true;
            }
        }

        public string ToPacketString()
        {
            string packet;
            packet = code.ToString("000") + " " + hop.ToString();

            if (data != null && data.Length > 0)
            {
                packet += " " + string.Join(":", data); 
            }

            return packet;
        }
    }
}
