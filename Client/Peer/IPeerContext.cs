using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.App;
using Client.Client;
using Client.Common.General;
using Client.Common.Net;

namespace Client.Peer
{
    /// <summary>
    /// EPSPの基底イベントデータクラスです。
    /// </summary>
    public abstract class EPSPDataEventArgs : EventArgs, INotifyPropertyChanged
    {
        /// <summary>受信したプロトコル日時を表します。</summary>
        public DateTime ReceivedAt { get; set; }
        /// <summary>署名の期限・内容が妥当かどうかを表します。</summary>
        public bool IsValid { get { return !IsInvalidSignature && !IsExpired; } }
        /// <summary>不正な署名であるかを表します。</summary>
        public bool IsInvalidSignature { get; set; } = true;
        /// <summary>署名が期限切れであるかを表します。</summary>
        public bool IsExpired { get; set; } = true;

        // TODO: FIXME: 本来ここに置くべきではない気がする。役割がおかしい。
        public event PropertyChangedEventHandler PropertyChanged;
        protected void CallPropertyChanged(string propertyName = "ReceivedAt")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 日本への津波の心配を表す列挙型です。
    /// </summary>
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

    /// <summary>地震情報の種類を表す列挙型です。</summary>
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

    /// <summary>震度観測点の情報を表すクラスです。</summary>
    public class QuakeObservationPoint
    {
        /// <summary>都道府県</summary>
        public string Prefecture { get; set; } = "";
        /// <summary>震度</summary>
        public string Scale { get; set; } = "";
        /// <summary>観測点名称</summary>
        public string Name { get; set; } = "";

        public int ScaleInt
        {
            get => Scale switch
            {
                "7" => 70,
                "6強" => 60,
                "6弱" => 55,
                "5強" => 50,
                "5弱" => 45,
                "4" => 40,
                "3" => 30,
                "2" => 20,
                "1" => 10,
                "5弱以上（推定）" => 46,
                _ => -1,
            };
        }
    }

    /// <summary>地震情報のイベントデータクラスです。</summary>
    public class EPSPQuakeEventArgs : EPSPDataEventArgs
    {
        /// <summary>発生日時</summary>
        public string OccuredTime { get; set; } = "不明";
        /// <summary>震度</summary>
        public string Scale { get; set; } = "不明";
        /// <summary>津波の有無</summary>
        public DomesticTsunamiType TsunamiType { get; set; } = DomesticTsunamiType.Unknown;
        /// <summary>地震情報種類</summary>
        public QuakeInformationType InformationType { get; set; } = QuakeInformationType.Unknown;
        /// <summary>震源</summary>
        public string Destination { get; set; } = "不明";
        /// <summary>深さ</summary>
        public string Depth { get; set; } = "不明";
        /// <summary>マグニチュード</summary>
        public string Magnitude { get; set; } = "不明";
        /// <summary>震度訂正かどうか</summary>
        public bool IsCorrection { get; set; } = false;
        /// <summary>緯度</summary>
        public string Latitude { get; set; } = "不明";
        /// <summary>経度</summary>
        public string Longitude { get; set; } = "不明";
        /// <summary>発表管区</summary>
        public string IssueFrom { get; set; } = "不明";
        /// <summary>地震情報詳細（震度観測点）</summary>
        public IList<QuakeObservationPoint> PointList { get; set; } = null;
    }

    /// <summary>津波予報の種類を表す列挙型です。</summary>
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

    /// <summary>予報区ごとの津波予報を表すクラスです。</summary>
    public class TsunamiForecastRegion
    {
        /// <summary>津波予報種類</summary>
        public TsunamiCategory Category { get; set; } = TsunamiCategory.Unknown;
        /// <summary>予報区名</summary>
        public string Region { get; set; } = "不明";
        /// <summary>津波が直ちに来襲するか</summary>
        public bool IsImmediately { get; set; } = false;
    }

    /// <summary>津波予報のイベントデータクラスです。</summary>
    public class EPSPTsunamiEventArgs : EPSPDataEventArgs
    {
        /// <summary>津波予報の解除かどうか</summary>
        public bool IsCancelled { get; set; } = false;
        /// <summary>津波予報詳細（予報区ごとの情報）</summary>
        public IList<TsunamiForecastRegion> RegionList { get; set; } = null;
    }

    /// <summary>地域ピア数のイベントデータクラスです。</summary>
    public class EPSPAreapeersEventArgs : EPSPDataEventArgs
    {
        /// <summary>地域ピア数 (キー: 地域コード、値: ピア数)</summary>
        public IDictionary<string, int> AreaPeerDictionary { get; set; } = null;
    }

    /// <summary>緊急地震速報 配信試験（オープンβ）のイベントデータクラスです。</summary>
    public class EPSPEEWTestEventArgs : EPSPDataEventArgs
    {
        /// <summary>テストかどうか</summary>
        public bool IsTest { get; set; } = false;
    }

    /// <summary>地震感知情報のイベントデータクラスです。</summary>
    public class EPSPUserquakeEventArgs : EPSPDataEventArgs
    {
        /// <summary>地域コード</summary>
        public string AreaCode { get; set; } = "";
        /// <summary>公開鍵</summary>
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
        /// 緊急地震速報 配信試験(β)イベント
        /// </summary>
        event EventHandler<EPSPEEWTestEventArgs> OnEEWTest;

        /// <summary>
        /// 地震感知情報イベント
        /// </summary>
        event EventHandler<EPSPUserquakeEventArgs> OnUserquake;

        /// <summary>
        /// データ受信イベント
        /// </summary>
        event EventHandler<EPSPRawDataEventArgs> OnData;

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
