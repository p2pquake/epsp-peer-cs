using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Client;
using Client.Common.General;
using Client.Common.Net;

namespace Client.Peer
{
    public abstract class EPSPDataEventArgs : EventArgs
    {
        /// <summary>署名の妥当性チェック結果</summary>
        public bool IsValid { get { return !IsInvalidSignature && !IsExpired; } }
        public bool IsInvalidSignature { get; set; } = true;
        public bool IsExpired { get; set; } = true;
    }

    public enum DomesticTsunamiType
    {
        /// <summary>なし</summary>
        None = 0,
        /// <summary>あり</summary>
        Effective = 1,
        /// <summary>調査中</summary>
        Checking = 2,
        /// <summary>不明</summary>
        Unknown = 3
    }

    public enum QuakeInformationType
    {
        /// <summary>震度速報</summary>
        ScalePrompt = 1,
        /// <summary>震源に関する情報</summary>
        Destination = 2,
        /// <summary>震源・震度に関する情報</summary>
        ScaleAndDestination = 3,
        /// <summary>各地の震度に関する情報</summary>
        Detail = 4,
        /// <summary>遠地地震に関する情報</summary>
        Foreign = 5,
        /// <summary>不明</summary>
        Unknown
    }

    public class QuakeObservationPoint
    {
        public string Prefecture { get; set; } = "";
        public string Scale { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class EPSPQuakeEventArgs : EPSPDataEventArgs
    {
        public string OccuredTime { get; set; } = "不明";
        public string Scale { get; set; } = "不明";
        public DomesticTsunamiType TsunamiType { get; set; } = DomesticTsunamiType.Unknown;
        public QuakeInformationType InformationType { get; set; } = QuakeInformationType.Unknown;
        public string Destination { get; set; } = "不明";
        public string Depth { get; set; } = "不明";
        public string Magnitude { get; set; } = "不明";
        public bool IsCorrection { get; set; } = false;
        public string Latitude { get; set; } = "不明";
        public string Longitude { get; set; } = "不明";
        public string IssueFrom { get; set; } = "不明";
        public IList<QuakeObservationPoint> PointList { get; set; } = null;
    }

    public enum TsunamiCategory
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

    public class TsunamiForecastRegion
    {
        public TsunamiCategory Category { get; set; } = TsunamiCategory.Unknown;
        public string Region { get; set; } = "不明";
        public bool IsImmediately { get; set; } = false;
    }

    public class EPSPTsunamiEventArgs : EPSPDataEventArgs
    {
        public bool IsCancelled { get; set; } = false;
        public IList<TsunamiForecastRegion> RegionList { get; set; } = null;
    }

    public class EPSPAreapeersEventArgs : EPSPDataEventArgs
    {
        public IDictionary<string, int> AreaPeerDictionary { get; set; } = null;
    }

    public class EPSPUserquakeEventArgs : EPSPDataEventArgs
    {
        public string AreaCode { get; set; } = "";
        public string PublicKey { get; set; } = "";
    }

    public class EPSPRawDataEventArgs
    {
        public string Packet { get; set; } = "";
    }

    /// <summary>
    /// 上位クラスへ見せるPeerContextインタフェース
    /// </summary>
    interface IPeerContext : IPeerConnector
    {
        IPeerConfig PeerConfig { set; }
        IPeerStateForPeer PeerState { set; }

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

#if RAISE_RAW_DATA_EVENT
        event EventHandler<EPSPRawDataEventArgs> OnData;
#endif

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
        /// すべてのピアにデータを配信します。
        /// </summary>
        /// <param name="packet">データ</param>
        void SendAll(Packet packet);

        /// <summary>
        /// 接続の受け入れを終了します。
        /// </summary>
        /// <returns>切断したかどうか</returns>
        bool EndListen();
    }
}
