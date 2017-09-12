using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.App
{
    public interface IOperatable
    {
        /// <summary>
        /// 接続処理を実行できる状態かどうか
        /// </summary>
        /// <returns></returns>
        bool CanConnect { get; }

        /// <summary>
        /// 切断処理を実行できる状態かどうか
        /// </summary>
        /// <returns></returns>
        bool CanDisconnect { get; }

        /// <summary>
        /// 接続する
        /// </summary>
        /// <returns></returns>
        bool Connect();

        /// <summary>
        /// 切断する
        /// </summary>
        /// <returns></returns>
        bool Disconnect();
    }
}
