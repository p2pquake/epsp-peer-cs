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

        private ILog logger = Logger.GetLog();

        private readonly int maintainInterval = 10000;
        private readonly int processTimeout = 90000;

        private IMediatorContext mediatorContext;
        private IClientContext clientContext;

        private Timer timer;
        private bool isStopped;

        public MaintainTimer(IMediatorContext mediatorContext, IClientContext clientContext)
        {
            this.mediatorContext = mediatorContext;
            this.clientContext = clientContext;

            timer = new Timer(Tick);
        }

        public void Start()
        {
            isStopped = false;
            timer.Change(0, maintainInterval);
        }

        private void Tick(object state)
        {
            logger.Debug("Tick");

            // Stopした後に動くことがないように（そういうことがあるのかわからんが）
            if (isStopped)
            {
                return;
            }

            if (mediatorContext.State is DisconnectedState)
            {
                RequireConnect(this, EventArgs.Empty);
            }
            else if (mediatorContext.State is ConnectedState)
            {
                // FIXME: 必要に応じてメンテナンス接続
                // FIXME: メンテ時に"IPAddress Changed"とか言われたときの考慮してない.以前の設計ではClient.State.IFinishedStateにもたせていたので、類似の方法で対応？
            }
            else if (mediatorContext.State is ConnectingState ||
                     mediatorContext.State is DisconnectingState ||
                     mediatorContext.State is MaintenanceState)
            {
                // FIXME: 長時間ingの場合は異常とみなしてDisconnectedへ遷移させる
            }
        }

        public void Stop()
        {
            isStopped = true;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            
            // FIXME: 切断処理が必要
        }
    }
}
