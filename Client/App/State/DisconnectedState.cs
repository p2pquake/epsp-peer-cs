using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Client;
using Client.Peer;

namespace Client.App.State
{
    /// <summary>
    /// 切断後の状態
    /// </summary>
    class DisconnectedState : AbstractState
    {
        internal override bool CanConnect
        {
            get { return true; }
        }

        internal override bool CanDisconnect
        {
            get { return false; }
        }

        internal override bool CanMaintain
        {
            get { return false; }
        }

        internal override bool Connect(MediatorContext mediatorContext, IClientContext clientContext, IPeerContext peerContext)
        {
            mediatorContext.State = new ConnectingState();
            return clientContext.Join();
        }
    }
}
