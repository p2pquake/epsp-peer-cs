using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;
using Client.Client.General;

namespace Client.Client.State
{
    abstract class AbstractState
    {
        public abstract void Process(Context context, CRLFSocket socket);

        /// <summary>
        /// 211 対応プロトコルバージョン等の要求
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual void RequireAllowVersion(Context context, CRLFSocket socket,  Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 212 対応プロトコルバージョン等の返答
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual void NoticeAllowVersion(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 233 ピアID暫定割り当て
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void AllocateTemporaryPeerId(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 234 ポート開放チェック結果
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void ReceivePortCheckResult(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 235 参加中のピアデータ
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void ReceiveJoiningPeerData(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 236 ピアID本割り当て
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void AllocatePeerId(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 237 鍵の割り当て
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void AllocateKey(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 238 プロトコル時刻
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void ReceiveProtocolTime(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 239 通信の終了
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void EndConnection(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 243 エコーOK
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void AcceptedEcho(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 244 鍵再割当て
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scket"></param>
        /// <param name="packet"></param>
        public virtual void ReAllocateKey(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 247 地域ピア数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void NoticeAreaPeers(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 248 参加終了OK
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void AcceptedPart(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 291 原因不明のエラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void UnknownError(Context context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 292 バージョン非互換エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void IncompatibleError(Context context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 293 不正リクエストエラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void IncorrectRequestError(Context context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 294 サーバ過負荷エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void ServerUnavailableError(Context context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 295 鍵割り当て不可エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void KeyCantAllocateError(Context context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }
        
        /// <summary>
        /// 298 プロトコル仕様非準拠エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void DifferentSpecificationError(Context context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 299 IPアドレス不一致エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        public virtual void AddressChangedError(Context context, CRLFSocket socket, Packet packet)
        {
            socket.Close();

            context.State = new FinishedState(
                ClientConst.OperationResult.Restartable,
                ClientConst.ErrorCode.RETURNED_ADDRESS_CHANGED
                );
        }
    }
}
