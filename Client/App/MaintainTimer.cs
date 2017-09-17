using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using Client.Client;
using Client.Common.General;
using Client.Common.Net;

namespace Client.App
{
    class MaintainTimer
    {
        private readonly int maintainInterval = 30000;
        private readonly int processTimeout = 90000;

        private IMediatorContext mediatorContext;
        private IClientContext clientContext;

        private Timer timer;
        private bool isStopped;

        public MaintainTimer(IMediatorContext mediatorContext, IClientContext clientContext)
        {
            this.mediatorContext = mediatorContext;
            this.clientContext = clientContext;
            clientContext.StateChanged += ClientContext_StateChanged;
        }

        public void Start()
        {
            isStopped = false;

            timer = new Timer(Tick);
            timer.Change(0, maintainInterval);
        }

        private void Tick(object state)
        {
            // Stopした後に動くことがないように（そういうことがあるのかわからんが）
            if (isStopped)
            {
                return;
            }

            if (clientContext.ClientState == ClientState.Disconnected)
            {
                // TODO: FIXME: 接続する
            }
            else if (clientContext.ClientState == ClientState.Connected)
            {
                // TODO: FIXME: 必要に応じてメンテナンス接続
                // TODO: FIXME: メンテ時に"IPAddress Changed"とか言われたときの考慮してない.以前の設計ではClient.State.IFinishedStateにもたせていたので、類似の方法で対応？
            }
            else if (clientContext.ClientState == ClientState.Connecting ||
                     clientContext.ClientState == ClientState.Disconnecting ||
                     clientContext.ClientState == ClientState.Maintaining)
            {
                // TODO: FIXME: 長時間ingの場合は異常とみなしてDisconnectedへ遷移させる
            }
        }

        public void Stop()
        {
            isStopped = true;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void ClientContext_StateChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
