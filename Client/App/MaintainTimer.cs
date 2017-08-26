using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using Client.Common.General;
using Client.Common.Net;

namespace Client.App
{
    class MaintainTimer
    {
        private static readonly int MAINTAIN_INTERVAL = 30000;
        private static readonly int WATCHDOG_COUNT = (90 * 1000) / MAINTAIN_INTERVAL;

        private MediatorContext context;
        private DateTime latestEcho;
        private int processingWatchdogCount;

        public MaintainTimer(MediatorContext p_context)
        {
            context = p_context;
            processingWatchdogCount = 0;
            latestEcho = DateTime.MinValue;
            context.StateChanged += new EventHandler(context_StateChanged);
        }

        void context_StateChanged(object sender, EventArgs e)
        {
            if (context.State is State.ConnectedState)
            {
                Logger.GetLog().Debug("エコーが行われました。");
                latestEcho = DateTime.Now;
            }
            if (context.State is State.DisconnectingState || context.State is State.DisconnectedState)
            {
                latestEcho = DateTime.MinValue;
            }
        }

        public void start()
        {
            Timer timer = new Timer(execTimer, null, 0, 30000);
        }
        
        private void execTimer(Object state)
        {
            // TODO: メンテナンスタイマ、今は雑な実装のみ

            // TODO: 簡易ウォッチドッグ・タイマ実装
            if (context.State is State.DisconnectingState || context.State is State.ConnectingState || context.State is State.MaintenanceState)
            {
                processingWatchdogCount++;

                if (processingWatchdogCount >= WATCHDOG_COUNT)
                {
                    Logger.GetLog().Error("サーバ通信ハングアップを検出しました。");
                    Environment.Exit(1);
                    throw new TimeoutException("サーバとの接続処理中のまま一定時間が経過しました（フリーズ？）。");
                }
            }
            else
            {
                processingWatchdogCount = 0;
            }

            // Logger.Write("状態: " + context.State + " (接続操作: " + context.CanConnect() + ", 切断操作: " + context.CanDisconnect() + ")", Logger.LogLevel.L7_DEBUG);
            StringBuilder sb = new StringBuilder();
            sb.Append("状態: " + context.State);
            sb.Append(", 接続数: " + context.ClientContext_GetCurrentConnection());
            sb.Append(" (");
            sb.Append("接続操作: " + context.CanConnect + ", ");
            sb.Append("切断操作: " + context.CanDisconnect + ", ");
            sb.Append("維持操作: " + context.CanMaintain);
            sb.Append(")");

            Logger.GetLog().Debug(sb.ToString());
            //Logger.GetLog().Debug("

            // TODO: ずっと接続維持するやつ
            if (context.CanConnect)
            {
                Logger.GetLog().Info("未接続状態のため、接続します。");
                context.Connect();
                return;
            }

            if (latestEcho == DateTime.MinValue)
            {
                Logger.GetLog().Debug("接続状態ではありません。");
                return;
            }

            Packet packet = new Packet();
            packet.Code = 611;
            packet.Hop = 1;
            context.PeerContext.SendAll(packet);

            // TODO: エコー間隔をハードコーディング
            int echoIntervalMinute = 12;
            if (context.ClientContext_GetCurrentConnection() < 3)
            {
                echoIntervalMinute = 3;
            }
            if (context.ClientContext_GetCurrentConnection() < 2)
            {
                echoIntervalMinute = 2;
            }

            TimeSpan progress = DateTime.Now - latestEcho;
            if (progress.TotalMinutes >= echoIntervalMinute)
            {
                if (context.CanMaintain)
                {
                    context.Maintain();
                }
            }
        }
    }
}
