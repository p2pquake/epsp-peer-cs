using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
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
        public event EventHandler RequireDisconnectAllPeers;

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
        private int retryCount = 0;

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
                retryCount = 0;
                return;
            }

            retryCount++;

            if (e.Result == Client.General.ClientConst.OperationResult.Restartable)
            {
                if (isStopped || retryCount > 10)
                {
                    return;
                }

                Task.Run(() =>
                {
                    RequireDisconnectAllPeers(this, EventArgs.Empty);
                    RequireConnect(this, EventArgs.Empty);
                });
            }
            if (e.Result == Client.General.ClientConst.OperationResult.Retryable)
            {
                if (isStopped)
                {
                    if (retryCount > 2)
                    {
                        mediatorContext.State = new DisconnectedState();
                        return;
                    }

                    Task.Run(() =>
                    {
                        RequireDisconnect(this, EventArgs.Empty);
                    });
                    return;
                }
                
                if (retryCount > 10)
                {
                    return;
                }
                
                // isStoppedでない: ConnectかMaintain なので絞りこめる
                if (mediatorContext.State is ConnectedState)
                {
                    Task.Run(() =>
                    {
                        RequireMaintain(this, EventArgs.Empty);
                    });
                }
                if (mediatorContext.State is DisconnectedState)
                {
                    Task.Run(() =>
                    {
                        RequireConnect(this, EventArgs.Empty);
                    });
                }
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

                if (echoElapsedCount > echoIntervalThreshold || (mediatorContext.Connections < 2 && echoElapsedCount > echoIntervalThresholdIfLackOfConnection))
                {
                    RequireMaintain(this, EventArgs.Empty);
                }
            }
            else if (mediatorContext.State is ConnectingState ||
                     mediatorContext.State is DisconnectingState ||
                     mediatorContext.State is MaintenanceState)
            {
                processingCount++;
                if (processingCount > processTimeoutCount)
                {
                    logger.Warn("サーバ通信が中断されました。後ほど再接続します。");
                    RequireDisconnectAllPeers(this, EventArgs.Empty);
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
