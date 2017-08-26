using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.App.State
{
    abstract class AbstractState
    {
        internal abstract bool CanConnect { get; }
        internal abstract bool CanDisconnect { get; }
        internal abstract bool CanMaintain { get; }

        internal virtual bool Connect(MediatorContext mediatorContext, Client.Context clientContext, Peer.Context peerContext)
        {
            throw new NotImplementedException();
        }

        internal virtual void Disconnect(MediatorContext mediatorContext, Client.Context clientContext, Peer.Context peerContext)
        {
            throw new InvalidOperationException();
        }

        internal virtual bool Maintain(MediatorContext mediatorContext, Client.Context clientContext, Peer.Context peerContext)
        {
            throw new InvalidOperationException();
        }

        internal virtual void Completed(MediatorContext mediatorContext, Client.Context clientContext, Peer.Context peerContext)
        {
            throw new NotImplementedException();
        }

        internal virtual void Abort(MediatorContext mediatorContext, Client.Context clientContext, Peer.Context peerContext)
        {
            throw new NotImplementedException();
        }
    }
}
