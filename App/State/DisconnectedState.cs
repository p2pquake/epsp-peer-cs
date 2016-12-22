using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        internal override bool Connect(MediatorContext mediatorContext, Client.Context clientContext, Peer.Context peerContext)
        {
            mediatorContext.State = new ConnectingState();

            bool result = clientContext.Join();
            if (!result)
            {
                mediatorContext.State = new DisconnectedState();
            }

            return result;
        }
    }
}
