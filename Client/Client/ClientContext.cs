using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Client.App;
using Client.Client.General;
using Client.Client.State;
using Client.Common.General;
using Client.Common.Net;
using Client.Peer;

namespace Client.Client
{
    class ClientContext : IClientContext, IClientContextForState
    {
        private AbstractState state;

        public AbstractState State {
            get { return state; }
            set {
                state = value;
                RaiseEvent();
            }
        }

        public IPeerConfig PeerConfig { get; set; }

        public IPeerConnector PeerConnector { get; set; }

        public IPeerStateForClient PeerState { get; set; }

        public event EventHandler StateChanged = (s, e) => { };
        public event EventHandler<OperationCompletedEventArgs> OperationCompleted = (s, e) => { };

        private List<ServerPoint> serverPointList = new List<ServerPoint>();
        private Random random = new Random();
        
        public bool IsOperatable
        {
            get { return State is IFinishedState; }
        }

        public ClientContext()
        {
            state = new FinishedState(ClientConst.OperationResult.Successful, ClientConst.ErrorCode.SUCCESSFUL);

            string[] servers = Application.Default.servers.Split(',');
               // ConfigurationManager.AppSettings["servers"].Split(',');
            foreach (string server in servers)
            {
                string[] items = server.Split(':');
                serverPointList.Add(new ServerPoint(items[0], int.Parse(items[1])));
            }
        }

        public bool Join()
        {
            bool result = Connect(new ConnectedState(ClientConst.ProcessType.Join));

            if (!result)
                State = new FinishedState(ClientConst.OperationResult.Retryable, ClientConst.ErrorCode.CONNECTION_FAILED);

            return result;
        }

        public bool Maintain()
        {
            bool result = Connect(new ConnectedState(ClientConst.ProcessType.Maintain));

            if (!result)
                State = new FinishedState(ClientConst.OperationResult.Retryable, ClientConst.ErrorCode.CONNECTION_FAILED);

            return result;
        }

        public bool Part()
        {
            bool result = Connect(new ConnectedState(ClientConst.ProcessType.Part));

            if (!result)
                State = new FinishedState(ClientConst.OperationResult.Retryable, ClientConst.ErrorCode.CONNECTION_FAILED);

            return result;
        }


        /// <summary>
        /// 接続する
        /// </summary>
        /// <returns></returns>
        private bool Connect(AbstractState state)
        {
            if (!IsOperatable)
                return false;

            State = state;

            CRLFSocket socket = new CRLFSocket();
            socket.ReadLine += new EventHandler<ReadLineEventArgs>(ProcessData);
            // FIXME: Closedに対する処理は未実装

            ServerPoint server = serverPointList[random.Next(serverPointList.Count)];
            return socket.Connect(server.Host, server.Port);
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
            string methodName = ClientConst.GetCodeName(packet.Code);

            Logger.GetLog().Debug("実行するメソッド: " + methodName);

            // HACK: こんなところでリフレクションを無駄に使うのはいかんでしょう。
            Type type = State.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName);

            object[] args = { this, socket, packet };

            methodInfo.Invoke(State, args);

            Logger.GetLog().Debug("実行後の状態: " + State.GetType().Name);

            State.Process(this, socket);
        }
        
        private void RaiseEvent()
        {
            if (state is IFinishedState)
            {
                IFinishedState finishedState = (IFinishedState)state;
                OperationCompletedEventArgs e = new OperationCompletedEventArgs(finishedState.Result, finishedState.ErrorCode);
                OperationCompleted(this, e);
            }
        }
    }
}
