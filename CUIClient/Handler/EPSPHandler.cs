using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Misc.UQSummary;
using Client.Peer;
using log4net;

namespace CUIClient.Handler
{
    /// <summary>
    /// EPSPの情報を処理するサンプルクラスです。
    /// </summary>
    public class EPSPHandler
    {
        private IUQManager uqManager;

        /// <param name="protocolTime">プロトコル日時</param>
        public EPSPHandler(Func<DateTime> protocolTime)
        {
            uqManager = new UQManager();
            uqManager.UQJudge = new SimpleUQJudge(3);
            uqManager.ProtocolTime = protocolTime;
            uqManager.Occurred += UqManager_Occurred;
            uqManager.Updated += UqManager_Updated;
        }

        /// <summary>
        /// 地震感知情報の更新イベント処理
        /// 
        /// <seealso cref="IUQManager.Updated"/>
        /// </summary>
        private void UqManager_Updated(object sender, EventArgs e)
        {
            Console.WriteLine("{0} 地震感知情報が更新されました: {1}", GetDateTime(), GetSummarizedUserquake());
        }

        /// <summary>
        /// 地震感知情報の発生(表示しきい値超え)イベント処理
        /// 
        /// <seealso cref="IUQManager.Occurred"/>
        /// </summary>
        private void UqManager_Occurred(object sender, EventArgs e)
        {
            Console.WriteLine("{0} 地震感知情報の受信しきい値を超えました: {1}", GetDateTime(), GetSummarizedUserquake());
        }

        /// <summary>
        /// 地震感知情報の簡易文字列化
        /// 
        /// <para>返却値は「ＸＸ北部(4)、ＸＸ南部(5)」のように、地域名と件数とを結合したものです。</para>
        /// </summary>
        private string GetSummarizedUserquake()
        {
            var uqSummary = uqManager.GetCurrentSummary();
            return string.Join(
                "、",
                uqSummary
                    .Where(item => AreaConverter.GetArea(item.Key) != null)
                    .Select(item => String.Format("{0}({1})", AreaConverter.GetArea(item.Key).Name, item.Value))
                );
        }

        /// <summary>
        /// 地震感知情報のイベント処理
        /// </summary>
        public void MediatorContext_OnUserquake(object sender, Client.Peer.EPSPUserquakeEventArgs e)
        {
            if (!e.IsValid) { return; }
            uqManager.Add(e.AreaCode);
        }
        
        /// <summary>
        /// 津波予報のイベント処理
        /// </summary>
        public void MediatorContext_OnTsunami(object sender, Client.Peer.EPSPTsunamiEventArgs e)
        {
            if (!e.IsValid) { return; }
            
            if (e.IsCancelled)
            {
                Console.WriteLine("{0} 津波予報は解除されました。", GetDateTime());
                return;
            }
            
            // 「[津波警報]ＸＸ県太平洋沿岸、[津波注意報]ＸＸ県、[津波注意報]ＸＸ県」のように出力します。
            Console.WriteLine(
                "{0} 津波予報を受信しました: {1}", 
                GetDateTime(),
                String.Join("、", e.RegionList.Select(item => String.Format("[{0}]{1}", GetTsunamiCategoryName(item.Category), item.Region)))
            );
        }

        /// <summary>
        /// 地震情報のイベント処理
        /// </summary>
        public void MediatorContext_OnEarthquake(object sender, Client.Peer.EPSPQuakeEventArgs e)
        {
            if (!e.IsValid) { return; }
            if (e.InformationType == QuakeInformationType.Unknown) { return; }

            var sb = new StringBuilder();
            sb.AppendFormat("{0} ", e.OccuredTime);

            // 最大震度
            if (e.InformationType == QuakeInformationType.Detail ||
                e.InformationType == QuakeInformationType.ScaleAndDestination ||
                e.InformationType == QuakeInformationType.ScalePrompt)
            {
                sb.AppendFormat("震度{0} ", e.Scale);
            }

            // 震源情報
            if (e.InformationType == QuakeInformationType.Destination ||
                e.InformationType == QuakeInformationType.Detail ||
                e.InformationType == QuakeInformationType.Foreign ||
                e.InformationType == QuakeInformationType.ScaleAndDestination)
            {
                sb.AppendFormat("{0}({1}, M{2}) ", e.Destination, e.Depth, e.Magnitude);
                sb.AppendFormat("{0}", GetDomesticTsunamiTypeName(e.TsunamiType));
            }

            Console.WriteLine("{0} 地震情報を受信しました: {1}", GetDateTime(), sb.ToString());
        }

        /// <summary>
        /// 地域ピア数のイベント処理
        /// </summary>
        public void MediatorContext_OnAreapeers(object sender, Client.Peer.EPSPAreapeersEventArgs e)
        {
            if (!e.IsValid) { return; }
            Console.WriteLine("{0} 地域ピア数の情報を受信しました: ピア数 {1}", GetDateTime(), e.AreaPeerDictionary.Sum(item => item.Value));
        }

        /// <summary>
        /// 緊急地震速報 配信試験（オープンβ）のイベント処理
        /// </summary>
        public void MediatorContext_OnEEWTest(object sender, Client.Peer.EPSPEEWTestEventArgs e)
        {
            if (!e.IsValid) { return; }
            if (e.IsTest) { return; }

            Console.WriteLine("{0} 緊急地震速報 配信試験（オープンβ）の情報を受信しました。", GetDateTime());
        }

        private string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
        }

        /// <summary>津波予報種類を日本語に変換します。</summary>
        private string GetTsunamiCategoryName(TsunamiCategory tsunamiCategory)
        {
            if (tsunamiCategory == TsunamiCategory.Advisory)
            {
                return "津波注意報";
            }
            if (tsunamiCategory == TsunamiCategory.Warning)
            {
                return "津波警報";
            }
            if (tsunamiCategory == TsunamiCategory.MajorWarning)
            {
                return "大津波警報";
            }
            return "不明";
        }

        /// <summary>津波の有無を日本語に変換します。</summary>
        private string GetDomesticTsunamiTypeName(DomesticTsunamiType type)
        {
            if (type == DomesticTsunamiType.Checking)
            {
                return "津波有無は調査中";
            }
            if (type == DomesticTsunamiType.Effective)
            {
                return "津波あり";
            }
            if (type == DomesticTsunamiType.None)
            {
                return "津波の心配なし";
            }
            return "津波有無は不明";
        }
    }
}
