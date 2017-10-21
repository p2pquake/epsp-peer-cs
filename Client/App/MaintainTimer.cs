using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using Client.App.State;
using Client.Client;
using Client.Common.General;
using Client.Common.Net;
using log4net;

namespace Client.App
{
    class MaintainTimer
    {
        public event EventHandler RequireConnect;
        public event EventHandler RequireMaintain;
        public event EventHandler RequireDisconnect;

        private ILog logger = Logger.GetLog();

        private readonly int maintainInterval = 10000;
        private readonly int processTimeoutCount = 9;
        private readonly int echoIntervalThreshold = 60;
        private readonly int echoIntervalThresholdIfLackOfConnection = 6;

        private IMediatorContext mediatorContext;
        private IClientContext clientContext;

        private Timer timer;
        private bool isStopped;

        private int processingCount = 0;
        private int echoElapsedCount = 0;

        public MaintainTimer(IMediatorContext mediatorContext, IClientContext clientContext)
        {
            this.mediatorContext = mediatorContext;
            this.clientContext = clientContext;

            mediatorContext.Completed += MediatorContext_Completed;

            timer = new Timer(Tick);
        }

        private void MediatorContext_Completed(object sender, OperationCompletedEventArgs e)
        {
            if (e.Result == Client.General.ClientConst.OperationResult.Successful)
            {
                echoElapsedCount = 0;
            }
        }

        public void Start()
        {
            isStopped = false;
            timer.Change(0, maintainInterval);
        }

        private void Tick(object state)
        {
            logger.Debug("Tick");
            echoElapsedCount++;

            // Stopした後に動くことがないように（そういうことがあるのかわからんが）
            if (isStopped)
            {
                return;
            }

            if (mediatorContext.State is DisconnectedState)
            {
                processingCount = 0;

                RequireConnect(this, EventArgs.Empty);
            }
            else if (mediatorContext.State is ConnectedState)
            {
                processingCount = 0;

                // FIXME: 接続数が少ない判定がClientのStateとバラバラになっていると思われる
                if (echoElapsedCount > echoIntervalThreshold || (mediatorContext.Connections < 2 && echoElapsedCount > echoIntervalThresholdIfLackOfConnection))
                {
                    RequireMaintain(this, EventArgs.Empty);
                }
                // FIXME: メンテ時に"IPAddress Changed"とか言われたときの考慮してない.以前の設計ではClient.State.IFinishedStateにもたせていたので、類似の方法で対応？
            }
            else if (mediatorContext.State is ConnectingState ||
                     mediatorContext.State is DisconnectingState ||
                     mediatorContext.State is MaintenanceState)
            {
                processingCount++;
                if (processingCount > processTimeoutCount)
                {
                    // TODO: ゴミセッションをどうやって消すか...
                    logger.Warn("サーバ通信が中断されました。後ほど再接続します。");
                    mediatorContext.State = new DisconnectedState();
                }
            }
        }

        public void Stop()
        {
            isStopped = true;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            
            if (mediatorContext.State is ConnectedState)
            {
                RequireDisconnect(this, EventArgs.Empty);
            }
        }
    }
}
