using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Client;
using Client.Peer;

namespace Client.App.State
{
    /// <summary>
    /// 接続後の状態
    /// </summary>
    class ConnectedState : AbstractState
    {
        internal override bool CanConnect
        {
            get { return false; }
        }

        internal override bool CanDisconnect
        {
            get { return true; }
        }

        internal override bool CanMaintain
        {
            get { return true; }
        }

        internal override void Disconnect(MediatorContext mediatorContext, IClientContext clientContext, IPeerContext peerContext)
        {
            mediatorContext.State = new DisconnectingState();
            peerContext.DisconnectAll();
            clientContext.Part();
        }

        internal override bool Maintain(MediatorContext mediatorContext, IClientContext clientContext, IPeerContext peerContext)
        {
            mediatorContext.State = new MaintenanceState();

            bool result = clientContext.Maintain();
            if (!result)
            {
                mediatorContext.State = new ConnectedState();
            }

            return result;
        }
    }
}
