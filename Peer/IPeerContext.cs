using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Client;
using Client.Common.General;

namespace Client.Peer
{
    abstract class EPSPDataEventArgs : EventArgs
    {
        /// <summary>署名の妥当性チェック結果</summary>
        bool IsValid { get; set; } = false;
        bool IsInvalidSignature { get; set; } = false;
        bool IsExpired { get; set; } = false;
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
        /// <summary>震源に関する情報</summary>
        Destination,
        /// <summary>震源・震度に関する情報</summary>
        ScaleAndDestination,
        /// <summary>各地の震度に関する情報</summary>
        Detail,
        /// <summary>遠地地震に関する情報</summary>
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
        IList<QuakeObservationPoint> PointList { get; set; } = null;
    }

    enum TsunamiCategory
    {
        /// <summary>津波注意報</summary>
        Advisory,
        /// <summary>津波警報</summary>
        Warning,
        /// <summary>大津波警報</summary>
        MajorWarning,
        /// <summary>不明</summary>
        Unknown
    }

    class TsunamiForecastRegion
    {
        TsunamiCategory Category { get; set; } = TsunamiCategory.Unknown;
        string Region { get; set; } = "不明";
        bool IsImmediately { get; set; } = false;
    }

    class EPSPTsunamiEventArgs : EPSPDataEventArgs
    {
        bool IsCancelled { get; set; } = false;
        IList<TsunamiForecastRegion> RegionList { get; set; } = null;
    }
    
    class EPSPAreapeersEventArgs : EPSPDataEventArgs
    {
        IDictionary<string, int> AreaPeerDictionary { get; set; } = null;
    }

    class EPSPUserquakeEventArgs : EPSPDataEventArgs
    {
        string AreaCode { get; set; } = "";
        string PublicKey { get; set; } = "";
    }

    /// <summary>
    /// 対ピア通信を行うサブシステムのコンテキストインタフェース
    /// </summary>
    interface IPeerContext
    {
        IPeerState PeerState { get; set; }

        /// <summary>
        /// 地震情報イベント
        /// </summary>
        event EventHandler<EPSPQuakeEventArgs> OnEarthquake;

        /// <summary>
        /// 津波予報イベント
        /// </summary>
        event EventHandler<EPSPTsunamiEventArgs> OnTsunami;

        /// <summary>
        /// 地域ピア数イベント
        /// </summary>
        event EventHandler<EPSPAreapeersEventArgs> OnAreapeers;

        /// <summary>
        /// 地震感知情報イベント
        /// </summary>
        event EventHandler<EPSPUserquakeEventArgs> OnUserquake;

        /// <summary>
        /// すべてのピア接続を切断します。
        /// </summary>
        void DisconnectAll();

        /// <summary>
        /// 指定ポート番号で接続を受け入れます。
        /// </summary>
        /// <param name="port">ポート番号</param>
        /// <returns>リッスンできたかどうか</returns>
        bool Listen(int port);

        /// <summary>
        /// 接続の受け入れを終了します。
        /// </summary>
        /// <returns>切断したかどうか</returns>
        bool EndListen();
    }
}
