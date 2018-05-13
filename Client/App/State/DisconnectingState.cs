using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Client;
using Client.Client.General;
using Client.Peer;

namespace Client.App.State
{
    /// <summary>
    /// EPSPネットワークからの切断を試みている状態です。
    /// </summary>
    class DisconnectingState : AbstractState
    {
        internal override bool CanConnect
        {
            get { return false; }
        }

        internal override bool CanDisconnect
        {
            get { return false; }
        }

        internal override bool CanMaintain
        {
            get { return false; }
        }

        internal override void Completed(MediatorContext mediatorContext, IClientContext clientContext, IPeerContext peerContext, OperationCompletedEventArgs oce)
        {
            if (oce.Result == ClientConst.OperationResult.Retryable)
            {
                mediatorContext.State = new ConnectedState();
                return;
            }

            mediatorContext.State = new DisconnectedState();
        }
    }
}
