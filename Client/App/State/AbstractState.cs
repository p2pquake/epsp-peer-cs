using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Client;
using Client.Peer;

namespace Client.App.State
{
    public abstract class AbstractState
    {
        internal abstract bool CanConnect { get; }
        internal abstract bool CanDisconnect { get; }
        internal abstract bool CanMaintain { get; }

        internal virtual bool Connect(MediatorContext mediatorContext, IClientContext clientContext, IPeerContext peerContext)
        {
            throw new InvalidOperationException($"Cannot connect in {GetType().Name}");
        }

        internal virtual void Disconnect(MediatorContext mediatorContext, IClientContext clientContext, IPeerContext peerContext)
        {
            throw new InvalidOperationException($"Cannot disconnect in {GetType().Name}");
        }

        internal virtual bool Maintain(MediatorContext mediatorContext, IClientContext clientContext, IPeerContext peerContext)
        {
            throw new InvalidOperationException($"Cannot maintain in {GetType().Name}");
        }

        internal virtual void Completed(MediatorContext mediatorContext, IClientContext clientContext, IPeerContext peerContext, OperationCompletedEventArgs oce)
        {
            throw new NotImplementedException();
        }

        internal virtual void Abort(MediatorContext mediatorContext, IClientContext clientContext, IPeerContext peerContext)
        {
            throw new NotImplementedException();
        }
    }
}
