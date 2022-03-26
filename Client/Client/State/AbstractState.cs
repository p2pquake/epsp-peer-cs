using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;
using Client.Client.General;

namespace Client.Client.State
{
    public abstract class AbstractState
    {
        internal abstract void Process(IClientContextForState context, CRLFSocket socket);

        /// <summary>
        /// 211 対応プロトコルバージョン等の要求
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal virtual void RequireAllowVersion(IClientContextForState context, CRLFSocket socket,  Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 212 対応プロトコルバージョン等の返答
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal virtual void NoticeAllowVersion(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 233 ピアID暫定割り当て
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void AllocateTemporaryPeerId(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 234 ポート開放チェック結果
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void ReceivePortCheckResult(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 235 参加中のピアデータ
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void ReceiveJoiningPeerData(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 236 ピアID本割り当て
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void AllocatePeerId(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 237 鍵の割り当て
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void AllocateKey(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 238 プロトコル時刻
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void ReceiveProtocolTime(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 239 通信の終了
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void EndConnection(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 243 エコーOK
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void AcceptedEcho(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 244 鍵再割当て
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scket"></param>
        /// <param name="packet"></param>
        internal virtual void ReAllocateKey(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 247 地域ピア数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void NoticeAreaPeers(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 248 参加終了OK
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void AcceptedPart(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 291 原因不明のエラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void UnknownError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 292 バージョン非互換エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void IncompatibleError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 293 不正リクエストエラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void IncorrectRequestError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 294 サーバ過負荷エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void ServerUnavailableError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 295 鍵割り当て不可エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void KeyCantAllocateError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new NotSupportedException();
        }
        
        /// <summary>
        /// 298 プロトコル仕様非準拠エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void DifferentSpecificationError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            throw new InvalidOperationException($"Cannot be operated in {GetType().Name}");
        }

        /// <summary>
        /// 299 IPアドレス不一致エラー
        /// </summary>
        /// <param name="context"></param>
        /// <param name="socket"></param>
        /// <param name="packet"></param>
        internal virtual void AddressChangedError(IClientContextForState context, CRLFSocket socket, Packet packet)
        {
            socket.Close();

            context.State = new FinishedState(
                ClientConst.OperationResult.Restartable,
                ClientConst.ErrorCode.RETURNED_ADDRESS_CHANGED
                );
        }
    }
}
