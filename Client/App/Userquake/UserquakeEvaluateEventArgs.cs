using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.App.Userquake
{
    public interface IUserquakeEvaluation
    {
        /// <summary>開始日時</summary>
        DateTime StartedAt { get; }
        /// <summary>更新日時</summary>
        DateTime UpdatedAt { get; }
        /// <summary>件数</summary>
        int Count { get; }
        /// <summary>信頼度 (適合率)</summary>
        double Confidence { get; }
    }

    public class UserquakeEvaluateEventArgs : EventArgs, IUserquakeEvaluation
    {
        public DateTime StartedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public int Count { get; private set; }
        public double Confidence { get; private set; }

        public UserquakeEvaluateEventArgs(DateTime startedAt, DateTime updatedAt, int count, double confidence)
        {
            StartedAt = startedAt;
            UpdatedAt = updatedAt;
            Count = count;
            Confidence = confidence;
        }
    }
}
