﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Client.General;
using Client.Client.State;
using Client.Peer;

namespace Client.Client
{
    public enum ClientState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
        Maintaining,
    }

    public class OperationCompletedEventArgs : EventArgs
    {
        private ClientConst.OperationResult result;
        private ClientConst.ErrorCode errorCode;

        public ClientConst.OperationResult Result { get { return result; } }
        public ClientConst.ErrorCode ErrorCode { get { return errorCode; } }

        internal OperationCompletedEventArgs(ClientConst.OperationResult result, ClientConst.ErrorCode errorCode)
        {
            this.result = result;
            this.errorCode = errorCode;
        }
    }

    /// <summary>
    /// 上位クラスへ見せるClientContextインタフェース
    /// </summary>
    interface IClientContext
    {
        IPeerStateForClient PeerState { set; }

        IPeerConfig PeerConfig { set; }

        IPeerConnector PeerConnector { set; }

        AbstractState State { get; }

        event EventHandler StateChanged;
        event EventHandler<OperationCompletedEventArgs> OperationCompleted;

        /// <summary>
        /// 参加する
        /// </summary>
        /// <returns></returns>
        bool Join();

        /// <summary>
        /// 参加終了する
        /// </summary>
        /// <returns></returns>
        bool Part();

        /// <summary>
        /// 接続を維持する
        /// </summary>
        /// <returns></returns>
        bool Maintain();

        /// <summary>
        /// 処理を強制的に中断する
        /// </summary>
        void Abort(ClientConst.ErrorCode errorCode);
    }
}
