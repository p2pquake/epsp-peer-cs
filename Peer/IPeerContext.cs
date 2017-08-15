using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Common.General;

namespace Client.Peer
{
    abstract class EPSPDataEventArgs : EventArgs
    {
        bool IsValid { get; set; }
    }

    enum DomesticTsunamiType
    {
        /// <summary>なし</summary>
        None,
        /// <summary>あり</summary>
        Effective,
        /// <summary>調査中</summary>
        Checking,
        /// <summary>不明</summary>
        Unknown
    }

    enum QuakeInformationType
    {
        /// <summary>震度速報</summary>
        ScalePrompt,
        /// <summary>震源情報</summary>
        Destination,
        /// <summary>震度・震源情報</summary>
        ScaleAndDestination,
        /// <summary>各地の震度に関する情報</summary>
        Detail,
        /// <summary>遠地地震情報</summary>
        Foreign,
        /// <summary>不明</summary>
        Unknown
    }

    class QuakeObservationPoint
    {
        string Prefecture { get; set; } = "";
        string Scale { get; set; } = "";
        string Name { get; set; } = "";
    }

    class EPSPQuakeEventArgs : EPSPDataEventArgs
    {
        string OccuredTime { get; set; } = "不明";
        string Scale { get; set; } = "不明";
        DomesticTsunamiType TsunamiType { get; set; } = DomesticTsunamiType.Unknown;
        QuakeInformationType InformationType { get; set; } = QuakeInformationType.Unknown;
        string Destination { get; set; } = "不明";
        string Depth { get; set; } = "不明";
        string Magnitude { get; set; } = "不明";
        bool IsCorrection { get; set; } = false;
        string Latitude { get; set; } = "不明";
        string Longitude { get; set; } = "不明";
        string IssueFrom { get; set; } = "不明";
        IList<QuakeObservationPoint> PointList { get; set; }
    }



    /// <summary>
    /// 対ピア通信を行うサブシステムのコンテキストインタフェース
    /// </summary>
    interface IPeerContext
    {
        /// <summary>
        /// 最大接続数
        /// </summary>
        int MaxConnections { get; set; }

        /// <summary>
        /// 接続タイムアウト(秒)
        /// </summary>
        int ConnectionTimeoutSeconds { get; set; }

        /// <summary>
        /// 地震情報イベント
        /// </summary>
        event EventHandler<EPSPQuakeEventArgs> OnEarthquake;

        // TODO: FIXME: 津波予報、地震感知情報、各地域ピア数はまだ未定義

        /// <summary>
        /// 指定したピアへの接続を試行します。
        /// </summary>
        /// <param name="peers">ピア情報</param>
        /// <returns>接続したピアのピアID</returns>
        int[] Connect(PeerData[] peers);

        /// <summary>
        /// 現在のピア接続数を返します。
        /// </summary>
        /// <returns></returns>
        int GetNumberOfConnections();

        /// <summary>
        /// すべてのピア接続を切断します。
        /// </summary>
        void DisconnectAll();
    }
}
