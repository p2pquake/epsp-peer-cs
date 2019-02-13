using Client.Peer;
using EPSPWPFClient.Quake;
using EPSPWPFClient.Userquake;
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
            if (dataEventArgs is EPSPTsunamiEventArgs)
            {
                return
                    dataEventArgs.ReceivedAt.ToString("dd日HH時mm分") +
                    "受信 " +
                    "津波予報" +
                    (((EPSPTsunamiEventArgs)dataEventArgs).IsCancelled ? "（解除）" : "");
            }
            if (dataEventArgs is EPSPEEWTestEventArgs)
            {
                return 
                    dataEventArgs.ReceivedAt.ToString("dd日HH時mm分") +
                    "受信 " +
                    "緊急地震速報 配信試験（β）";
            }
            if (dataEventArgs is EPSPUQSummaryEventArgs)
            {
                var uqSummary = (EPSPUQSummaryEventArgs)dataEventArgs;
                return
                    uqSummary.StartedAt.ToString("dd日HH時mm分ss秒") +
                    "～" +
                    uqSummary.UpdatedAt.ToString("dd日HH時mm分ss秒") +
                    "受信 " +
                    "地震感知情報 計" +
                    uqSummary.Summary.Select(e => e.Value).Sum() + "件";
            }

            return "";
        }
    }
}
