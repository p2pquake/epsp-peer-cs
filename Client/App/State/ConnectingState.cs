﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Client;
using Client.Client.General;

using Client.Common.General;
using Client.Peer;

namespace Client.App.State
{
    /// <summary>
    /// EPSPネットワークへの接続を試みている状態です。
    /// </summary>
    class ConnectingState : AbstractState
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
            ClientConst.OperationResult operationResult = oce.Result;

            if (operationResult == ClientConst.OperationResult.Successful)
            {
                Logger.GetLog().Info("接続が完了しました（接続数： " + peerContext.Connections + "）。");

                mediatorContext.State = new ConnectedState();
            }
            else if (operationResult == ClientConst.OperationResult.Restartable)
            {
                Logger.GetLog().Info($"理由 {oce.ErrorCode.ToString()} により、再接続します。");

                peerContext.EndListen();
                peerContext.DisconnectAll();

                Logger.GetLog().Info("ピア接続をすべて切断しました（接続数: " + peerContext.Connections + "）");

                mediatorContext.State = new DisconnectedState();
                // MaintainTimer に任せる（一定のインターバルを置く）
                //mediatorContext.Connect();
            }
            else if (operationResult == ClientConst.OperationResult.Retryable)
            {
                // FIXME: 再試行すること。いまは放置プレイ
                Logger.GetLog().Info("接続に失敗しましたが、再試行可能なエラーです。");

                peerContext.EndListen();
                peerContext.DisconnectAll();

                mediatorContext.State = new DisconnectedState();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
