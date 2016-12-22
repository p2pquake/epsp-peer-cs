using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Client.General
{
    class ClientConst
    {
        public static string getCodeName(int code)
        {
            Dictionary<int, string> codeMap = new Dictionary<int,string>()
            {
                { 211, "RequireAllowVersion" },
                { 212, "NoticeAllowVersion" },
                { 233, "AllocateTemporaryPeerId" },
                { 234, "ReceivePortCheckResult" },
                { 235, "ReceiveJoiningPeerData" },
                { 236, "AllocatePeerId" },
                { 237, "AllocateKey" },
                { 238, "ReceiveProtocolTime" },
                { 239, "EndConnection" },
                { 243, "AcceptedEcho" },
                { 244, "ReAllocateKey" },
                { 247, "NoticeAreaPeers" },
                { 248, "AcceptedPart" },
                { 291, "UnknownError" },
                { 292, "IncompatibleError" },
                { 293, "IncorrectRequestError" },
                { 294, "ServerUnavailableError" },
                { 295, "KeyCantAllocateError" },
                { 298, "DifferentSpecificationError" },
                { 299, "AddressChangedError" }
            };

            return codeMap[code];
        }

        public enum OperationResult
        {
            /// <summary>
            /// 成功
            /// </summary>
            Successful,
            /// <summary>
            /// 再試行可能なエラー
            /// </summary>
            Retryable,
            /// <summary>
            /// 再参加が可能なエラー
            /// </summary>
            Restartable,
            /// <summary>
            /// 再試行不可能なエラー
            /// </summary>
            Fatal
        }

        /// <summary>
        /// エラーコード
        /// </summary>
        public enum ErrorCode
        {
            /// <summary>
            /// 成功
            /// </summary>
            SUCCESSFUL = 0,
            /// <summary>
            /// 接続に失敗した
            /// </summary>
            CONNECTION_FAILED = 1,
            /// <summary>
            /// 処理タイムアウト
            /// </summary>
            TIMED_OUT = 2,

            /// <summary>
            /// サーバ互換性なし
            /// </summary>
            RETURNED_INCOMPATIBLE_SERVER = 192,
            /// <summary>
            /// 不明なエラー
            /// </summary>
            RETURNED_UNKNOWN = 291,
            /// <summary>
            /// クライアント互換性なし
            /// </summary>
            RETURNED_INCOMPATIBLE_CLIENT = 292,
            /// <summary>
            /// 不正なリクエスト
            /// </summary>
            RETURNED_INVALID_REQUEST = 293,
            /// <summary>
            /// 過負荷またはメンテナンス中
            /// </summary>
            RETURNED_MAINTENANCE = 294,
            /// <summary>
            /// プロトコル仕様非準拠
            /// </summary>
            RETURNED_DIFF_SPEC = 298,
            /// <summary>
            /// IPアドレス変化
            /// </summary>
            RETURNED_ADDRESS_CHANGED = 299
        }

        /// <summary>
        /// 処理タイプ
        /// </summary>
        internal enum ProcessType
        {
            Join,
            Part,
            Maintain
        }
    }
}
