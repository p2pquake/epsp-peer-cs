using Client.Peer;
using EPSPWPFClient.Quake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPSPWPFClient.ViewModel
{
    class EPSPTitleConverter
    {
        internal static string GetTitle(EPSPDataEventArgs dataEventArgs)
        {
            if (dataEventArgs is EPSPQuakeEventArgs)
            {
                var quake = (EPSPQuakeEventArgs)dataEventArgs;
                var builder = new StringBuilder();

                builder.Append(quake.OccuredTime + " ");
                if (quake.InformationType == QuakeInformationType.Detail ||
                    quake.InformationType == QuakeInformationType.ScaleAndDestination ||
                    quake.InformationType == QuakeInformationType.ScalePrompt)
                {
                    builder.Append("震度" + quake.Scale + " ");
                }
                if (quake.InformationType == QuakeInformationType.Destination ||
                    quake.InformationType == QuakeInformationType.Detail ||
                    quake.InformationType == QuakeInformationType.Foreign ||
                    quake.InformationType == QuakeInformationType.ScaleAndDestination)
                {
                    builder.Append(quake.Destination + "(" + quake.Depth + ",M" + quake.Magnitude + ") ");
                    builder.Append(CodeMapper.ToString(quake.TsunamiType) + " ");
                }
                return builder.ToString();
            }
            // TODO: 日時表示 (#80)
            if (dataEventArgs is EPSPEEWTestEventArgs)
            {
                return "緊急地震速報 配信試験（β）";
            }

            return "";
        }
    }
}
