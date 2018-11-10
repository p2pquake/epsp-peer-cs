using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Misc.UQSummary;
using UserquakeSimulator.Reader;

namespace UserquakeSimulator.Evaluator
{
    public class Evaluator : IEvaluator
    {
        public IReader Reader { private get; set; }
        public IUQExtManager UQExtManager { private get; set; }
        public IUQJudge UQJudge { private get; set; }
        public IVerifier Verifier { private get; set; }

        private DateTime protocolTime;
        private IDictionary<string, int> areapeers;

        private int tp, tn, fp, fn;
        private TimeSpan elapsedSum;

        public EvaluateResult Evaluate()
        {
            // initialize
            UQExtManager.UQJudge = UQJudge;
            UQExtManager.AreaPeerDictionary = GetAreapeers;
            UQExtManager.ProtocolTime = GetProtocolTime;
            UQExtManager.Occurred += (s, e) => { };
            UQExtManager.Updated += (s, e) => { };
            UQExtManager.Initialized += UQExtManager_Initialized;

            tp = tn = fp = fn = 0;
            elapsedSum = TimeSpan.Zero;

            foreach (var data in Reader.GetAll())
            {
                if (data.DataType == DataType.Userquake)
                {
                    protocolTime = data.ArrivalTime;
                    UQExtManager.Add(data.AreaCode);
                }
                if (data.DataType == DataType.Areapeer)
                {
                    areapeers = data.PeerMap;
                }
            }

            protocolTime = DateTime.MaxValue;
            UQExtManager.Add("901");

            var result = new EvaluateResult()
            {
                TP = tp,
                FP = fp,
                TN = tn,
                FN = fn,
                AverageElapsed = TimeSpan.FromMilliseconds(elapsedSum.TotalMilliseconds / (tp + tn + fp + fn))
            };

            return result;
        }

        private void UQExtManager_Initialized(object sender, UQDataEventArgs e)
        {
            var map = new Dictionary<string, string>()
            {
                { "010", "北海道" },
                { "015", "北海道" },
                { "020", "北海道" },
                { "025", "北海道" },
                { "030", "北海道" },
                { "035", "北海道" },
                { "040", "北海道" },
                { "045", "北海道" },
                { "050", "北海道" },
                { "055", "北海道" },
                { "060", "北海道" },
                { "065", "北海道" },
                { "070", "北海道" },
                { "075", "北海道" },
                { "100", "青森県" },
                { "105", "青森県" },
                { "106", "青森県" },
                { "110", "岩手県" },
                { "111", "岩手県" },
                { "115", "岩手県" },
                { "120", "宮城県" },
                { "125", "宮城県" },
                { "130", "秋田県" },
                { "135", "秋田県" },
                { "140", "山形県" },
                { "141", "山形県" },
                { "142", "山形県" },
                { "143", "山形県" },
                { "150", "福島県" },
                { "151", "福島県" },
                { "152", "福島県" },
                { "200", "茨城県" },
                { "205", "茨城県" },
                { "210", "栃木県" },
                { "215", "栃木県" },
                { "220", "群馬県" },
                { "225", "群馬県" },
                { "230", "埼玉県" },
                { "231", "埼玉県" },
                { "232", "埼玉県" },
                { "240", "千葉県" },
                { "241", "千葉県" },
                { "242", "千葉県" },
                { "250", "東京都" },
                { "255", "東京都" },
                { "260", "東京都" },
                { "265", "東京都" },
                { "270", "神奈川県" },
                { "275", "神奈川県" },
                { "300", "新潟県" },
                { "301", "新潟県" },
                { "302", "新潟県" },
                { "305", "新潟県" },
                { "310", "富山県" },
                { "315", "富山県" },
                { "320", "石川県" },
                { "325", "石川県" },
                { "330", "福井県" },
                { "335", "福井県" },
                { "340", "山梨県" },
                { "345", "山梨県" },
                { "350", "長野県" },
                { "351", "長野県" },
                { "355", "長野県" },
                { "400", "岐阜県" },
                { "405", "岐阜県" },
                { "410", "静岡県" },
                { "411", "静岡県" },
                { "415", "静岡県" },
                { "416", "静岡県" },
                { "420", "愛知県" },
                { "425", "愛知県" },
                { "430", "三重県" },
                { "435", "三重県" },
                { "440", "滋賀県" },
                { "445", "滋賀県" },
                { "450", "京都府" },
                { "455", "京都府" },
                { "460", "大阪府" },
                { "465", "大阪府" },
                { "470", "兵庫県" },
                { "475", "兵庫県" },
                { "480", "奈良県" },
                { "490", "和歌山県" },
                { "495", "和歌山県" },
                { "500", "鳥取県" },
                { "505", "鳥取県" },
                { "510", "島根県" },
                { "515", "島根県" },
                { "514", "島根県" },
                { "520", "岡山県" },
                { "525", "岡山県" },
                { "530", "広島県" },
                { "535", "広島県" },
                { "540", "山口県" },
                { "545", "山口県" },
                { "541", "山口県" },
                { "550", "徳島県" },
                { "555", "徳島県" },
                { "560", "香川県" },
                { "570", "愛媛県" },
                { "575", "愛媛県" },
                { "576", "愛媛県" },
                { "580", "高知県" },
                { "581", "高知県" },
                { "582", "高知県" },
                { "600", "福岡県" },
                { "601", "福岡県" },
                { "602", "福岡県" },
                { "605", "福岡県" },
                { "610", "佐賀県" },
                { "615", "佐賀県" },
                { "620", "長崎県" },
                { "625", "長崎県" },
                { "630", "長崎県" },
                { "635", "長崎県" },
                { "640", "熊本県" },
                { "641", "熊本県" },
                { "645", "熊本県" },
                { "646", "熊本県" },
                { "650", "大分県" },
                { "651", "大分県" },
                { "655", "大分県" },
                { "656", "大分県" },
                { "660", "宮崎県" },
                { "661", "宮崎県" },
                { "665", "宮崎県" },
                { "666", "宮崎県" },
                { "670", "鹿児島県" },
                { "675", "鹿児島県" },
                { "680", "鹿児島県" },
                { "685", "鹿児島県" },
                { "700", "沖縄県" },
                { "701", "沖縄県" },
                { "702", "沖縄県" },
                { "710", "沖縄県" },
                { "706", "沖縄県" },
                { "705", "沖縄県" }
            };

            // 都道府県コンバータ
            var prefs = e.Summary.Where(item => map.ContainsKey(item.Key)).Select(item => map[item.Key]).Distinct().ToArray();
            var haveEq = Verifier.HaveEarthquake(
                    e.List.First().AbstractTime, e.List.Last().AbstractTime, prefs
                    );

            if (e.IsSatisfied && haveEq)
            {
                tp++;
            }
            if (e.IsSatisfied && !haveEq)
            {
                fp++;
            }
            if (!e.IsSatisfied && haveEq)
            {
                fn++;
            }
            if (!e.IsSatisfied && !haveEq)
            {
                tn++;
            }

            elapsedSum += e.Elapsed;

            //Console.WriteLine("----------------------------------------");
            //Console.WriteLine("IsSatisfied: " + e.IsSatisfied + ", haveEq:" + haveEq);
            //Console.WriteLine(string.Join(",", e.Summary.Select(v => v.Key + ":" + v.Value)));
            //Console.WriteLine("----------------------------------------");
        }

        private DateTime GetProtocolTime()
        {
            return protocolTime;
        }

        private IDictionary<string, int> GetAreapeers()
        {
            return areapeers;
        }
    }
}
