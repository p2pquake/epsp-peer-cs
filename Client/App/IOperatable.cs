using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.App
{
    /// <summary>
    /// 接続制御が行うためのインタフェースです。
    /// </summary>
    public interface IOperatable
    {
        /// <summary>
        /// 接続処理を実行できる状態かどうかを表します。
        /// </summary>
        /// <returns></returns>
        bool CanConnect { get; }

        /// <summary>
        /// 切断処理を実行できる状態かどうかを表します。
        /// </summary>
        /// <returns></returns>
        bool CanDisconnect { get; }

        /// <summary>
        /// EPSPネットワークに接続します。
        /// </summary>
        /// <returns></returns>
        bool Connect();

        /// <summary>
        /// EPSPネットワークから切断します。
        /// </summary>
        /// <returns></returns>
        bool Disconnect();
    }
}
