using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.App;
using Client.Client.General;
using Client.Common.Net;

namespace Client.Client.State
{
    class RequirePartState : AbstractState
    {
        internal override void Process(IClientContextForState context, CRLFSocket socket)
        {
            IPeerStateForClient peerState = context.PeerState;
            string[] datas = { peerState.PeerId.ToString(), (peerState.Key == null ? "Unknown" : peerState.Key.PublicKey) };

            socket.WriteLine("128 1 " + string.Join(":", datas));
        }

        internal override void AcceptedPart(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            context.State = new EndConnectionState(ClientConst.OperationResult.Successful, ClientConst.ErrorCode.SUCCESSFUL);
        }

        internal override void AddressChangedError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            context.State = new EndConnectionState(ClientConst.OperationResult.Restartable, ClientConst.ErrorCode.RETURNED_ADDRESS_CHANGED);
        }

        internal override void IncorrectRequestError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            context.State = new EndConnectionState(ClientConst.OperationResult.Restartable, ClientConst.ErrorCode.RETURNED_INVALID_REQUEST);
        }
    }
}
