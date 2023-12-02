namespace LegacyPluginSupporter.Net
{
    public class Packet
    {
        public string Code { get; set; }
        public string[] Data { get; set; }

        /// <summary>
        /// プラグインからの通信をパースします（プラグインへの通信はパースできません）
        /// </summary>
        public static Packet Parse(string line)
        {
            Packet packet = new();
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
            string[] datas = packet.Split(new char[] { ' ' }, 2);

            // 初期値
            Code = "";
            Data = null;

            if (datas.Length <= 0 || datas.Length >= 3)
            {
                return false;
            }
            else
            {
                Code = datas[0];
                if (datas.Length >= 2)
                {
                    Data = new string[] { datas[1] };
                }
                return true;
            }
        }

        public string ToPacketString()
        {
            string packet;
            packet = Code;

            if (Data != null && Data.Length > 0)
            {
                packet += " " + string.Join(":", Data);
            }

            return packet;
        }
    }
}
