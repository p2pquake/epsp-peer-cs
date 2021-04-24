using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.App.Userquake
{
    /// <summary>
    /// 地震感知情報の評価結果
    /// </summary>
    public interface IUserquakeEvaluation
    {
        /// <summary>開始日時</summary>
        DateTime StartedAt { get; }
        /// <summary>更新日時</summary>
        DateTime UpdatedAt { get; }
        /// <summary>件数</summary>
        int Count { get; }
        /// <summary>
        /// P2P地震情報 Beta3 互換の信頼度 (適合率)
        ///
        /// <list type="bullet">
        /// <item><term>0</term><description>非表示</description></item>
        /// <item><term>0.97015</term><description>レベル1</description></item>
        /// <item><term>0.96774</term><description>レベル2</description></item>
        /// <item><term>0.97024</term><description>レベル3</description></item>
        /// <item><term>0.98052</term><description>レベル4</description></item>
        /// </list>
        /// </summary>
        double Confidence { get; }
        /// <summary>
        /// P2P地震情報 Beta3 互換の信頼度レベル (1～4、非表示は 0)
        /// </summary>
        int ConfidenceLevel { get; }

        /// <summary>
        /// 地域毎の信頼度情報
        /// </summary>
        IReadOnlyDictionary<string, IUserquakeEvaluationArea> AreaConfidences { get; }
    }

    /// <summary>
    /// 地震感知情報 地域の信頼度情報
    /// </summary>
    public interface IUserquakeEvaluationArea
    {
        /// <summary>地域コード</summary>
        string AreaCode { get; }
        /// <summary>件数</summary>
        int Count { get; }
        /// <summary>
        /// P2P地震情報 Beta3 互換の信頼度
        ///
        /// <list type="bullet">
        /// <item><term>0未満</term><description>信頼度 F</description></item>
        /// <item><term>0.0以上0.2未満</term><description>信頼度 E</description></item>
        /// <item><term>0.2以上0.4未満</term><description>信頼度 D</description></item>
        /// <item><term>0.4以上0.6未満</term><description>信頼度 C</description></item>
        /// <item><term>0.6以上0.8未満</term><description>信頼度 B</description></item>
        /// <item><term>0.8以上</term><description>信頼度 A</description></item>
        /// </list>
        /// </summary>
        double Confidence { get; }
        /// <summary>
        /// P2P地震情報 Beta3 互換の信頼度ラベル (A～F)
        /// </summary>
        string ConfidenceLevel { get; }
    }

    public class UserquakeEvaluateEventArgs : EventArgs, IUserquakeEvaluation
    {
        public DateTime StartedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int Count { get; init; }
        public double Confidence { get; init; }
        public int ConfidenceLevel => Confidence switch
        {
            double x when x >= 0.9805 => 4,
            double x when x >= 0.9702 => 3,
            double x when x >= 0.9701 => 1, // 間違っていない. 2 のほうが適合率が低い
            double x when x >= 0.9677 => 2,
            _ => 0,
        };
        public IReadOnlyDictionary<string, IUserquakeEvaluationArea> AreaConfidences { get; init; }
    }

    public record UserquakeEvaluationArea : IUserquakeEvaluationArea
    {
        public string AreaCode { get; init; }
        public int Count { get; init; }
        public double Confidence { get; init; }
        public string ConfidenceLevel => Confidence switch
        {
            double x when x >= 0.8 => "A",
            double x when x >= 0.6 => "B",
            double x when x >= 0.4 => "C",
            double x when x >= 0.2 => "D",
            double x when x >= 0.0 => "E",
            _ => "F",
        };
    }
}
