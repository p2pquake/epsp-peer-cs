using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.Utils
{
    public static class ScaleConverter
    {
        public static int Str2Int(string scale)
        {
            return scale switch
            {
                "1" => 10,
                "2" => 20,
                "3" => 30,
                "4" => 40,
                "5弱" => 45,
                "5弱以上（推定）" => 46,
                "5強" => 50,
                "6弱" => 55,
                "6強" => 60,
                "7" => 70,
                _ => -1,
            };
        }

        public static string Int2Str(int scale)
        {
            return scale switch
            {
                10 => "1",
                20 => "2",
                30 => "3",
                40 => "4",
                45 => "5弱",
                46 => "5弱以上と推定",
                50 => "5強",
                55 => "6弱",
                60 => "6強",
                70 => "7",
                _ => "不明",
            };
        }
    }
}
