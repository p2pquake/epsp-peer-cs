using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Client.Client.General;
using Client.Client.State;
using Client.Common.Net;
using Client.Common.General;

namespace Client.Client
{
    class Context
    {
        private AbstractState state = new FinishedState(ClientConst.OperationResult.Successful, ClientConst.ErrorCode.SUCCESSFUL);

        public AbstractState State
        {
            get { return state; }
            set
            {
                state = value;
                StateChanged(this, EventArgs.Empty);

                if (state is IFinishedState)
                    OperationCompleted(this, EventArgs.Empty);
            }
        }

        /// <summary>ピアID</summary>
        public int PeerId { get; set; }
        /// <summary>ポート (-1: 開放しない)</summary>
        public int Port { get; set; }
        /// <summary>地域コード</summary>
        public int AreaCode { get; set; }
        /// <summary>最大接続数</summary>
        public int AllowConnection { get; set; }
        /// <summary>プロトコル時刻の時刻差</summary>
        public int TimeOffset { get; set; }
        /// <summary>鍵情報</summary>
        public KeyData Key { get; set; }

        public event EventHandler StateChanged = (s,e) => {};
        public event EventHandler OperationCompleted = (s,e) => { };

        /// <summary>接続先ピアリストの通知</summary>
        public Func<PeerData[], int[]> ConnectToPeers; // = (x) => { throw new Noti};
        /// <summary>参加ピア総数の通知</summary>
        public Action<int> NotifyCurrentPeers; // = (x) => { throw new NotImplementedException(); };
        /// <summary>接続数の取得</summary>
        public Func<int> GetCurrentConnection; // = () => { return 0; };
        
        /// <summary>
        /// 新規参加する
        /// </summary>
        /// <returns></returns>
        public bool Join()
        {
            bool result = Connect(new ConnectedState(ClientConst.ProcessType.Join));

            if (!result)
                State = new FinishedState(ClientConst.OperationResult.Retryable, ClientConst.ErrorCode.CONNECTION_FAILED);

            return result;
        }

        /// <summary>
        /// 参加終了する
        /// </summary>
        public void Part()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 接続維持する
        /// </summary>
        public bool Maintain()
        {
            bool result = Connect(new ConnectedState(ClientConst.ProcessType.Maintain));

            if (!result)
                State = new FinishedState(ClientConst.OperationResult.Retryable, ClientConst.ErrorCode.CONNECTION_FAILED);

            return result;
        }

        /// <summary>
        /// 処理を中断する
        /// </summary>
        public void Interrupt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 処理結果を返却する
        /// </summary>
        /// <returns></returns>
        public ClientConst.OperationResult getOperationResult()
        {
            if (state is IFinishedState)
            {
                IFinishedState finishedState = (IFinishedState)state;
                return finishedState.Result;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// 接続する
        /// </summary>
        /// <returns></returns>
        private bool Connect(AbstractState state)
        {
            if (!IsOperatable())
                return false;

            State = state;

            CRLFSocket socket = new CRLFSocket();
            socket.ReadLine += new EventHandler<ReadLineEventArgs>(ProcessData);
            // TODO: Closedに対する処理は未実装

            // TODO: 接続先はp2pquake.ddo.jp固定。本当は複数対応にしたい
            return socket.Connect("p2pquake.ddo.jp", 6910);
        }

        /// <summary>
        /// データを処理します。
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        private void ProcessData(Object sender, ReadLineEventArgs e)
        {
            Logger.GetLog().Debug("現在の状態: " + State.GetType().Name);

            CRLFSocket socket = (CRLFSocket)sender;
            Packet packet = e.packet;
            string methodName = ClientConst.getCodeName(packet.Code);

            Logger.GetLog().Debug("実行するメソッド: " + methodName);

            Type type = State.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName);

            object[] args = { this, socket, packet };

            methodInfo.Invoke(State, args);

            Logger.GetLog().Debug("実行後の状態: " + State.GetType().Name);

            State.Process(this, socket);
        }

        /// <summary>
        /// 操作可能な状態かどうかを返す
        /// </summary>
        /// <returns></returns>
        private bool IsOperatable()
        {
            return (State is IFinishedState);
        }
    }
}
