using Client.Peer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.Quake
{
    class CodeMapper
    {
        internal static string ToString(QuakeInformationType quakeInformationType)
        {
            if (quakeInformationType == QuakeInformationType.Destination)
            {
                return "震源情報";
            }
            if (quakeInformationType == QuakeInformationType.Detail)
            {
                return "地震情報";
            }
            if (quakeInformationType == QuakeInformationType.Foreign)
            {
                return "遠地地震情報";
            }
            if (quakeInformationType == QuakeInformationType.ScaleAndDestination)
            {
                return "震度・震源情報";
            }
            if (quakeInformationType == QuakeInformationType.ScalePrompt)
            {
                return "震度速報";
            }

            return "不明な情報";
        }

        internal static string ToString(DomesticTsunamiType domesticTsunamiType)
        {
            if (domesticTsunamiType == DomesticTsunamiType.Checking)
            {
                return "津波の有無は調査中";
            }
            if (domesticTsunamiType == DomesticTsunamiType.Effective)
            {
                return "津波予報発表中";
            }
            if (domesticTsunamiType == DomesticTsunamiType.None)
            {
                return "津波の心配なし";
            }

            return "津波の有無は不明";
        }
    }
}
