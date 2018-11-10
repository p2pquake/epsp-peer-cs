using Client.Misc.UQSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserquakeSimulator.Reader;

namespace UserquakeSimulator.Evaluator
{
    public class EvaluateResult
    {
        public int TP { get; set; }
        public int FP { get; set; }
        public int TN { get; set; }
        public int FN { get; set; }
        public TimeSpan AverageElapsed { get; set; }

        public double Accuracy
        {
            get { return (TP + TN) / (double)(TP + FP + FN + TN); }
        }
        public double ErrorRate
        {
            get { return 1 - Accuracy; }
        }
        public double Precision
        {
            get { return TP / (double)(TP + FP); }
        }
        public double Sensitivity
        {
            get { return TP / (double)(TP + FN); }
        }
        public double Specificity
        {
            get { return TN / (double)(TN + FP); }
        }
        public double FalsePositiveRate
        {
            get { return FP / (double)(TN + FP); }
        }
        public double FalseNegativeRate
        {
            get { return FN / (double)(TP + FN); }
        }
    }

    public interface IEvaluator
    {
        IReader Reader { set; }
        IUQExtManager UQExtManager { set; }
        IUQJudge UQJudge { set; }
        IVerifier Verifier { set; }

        EvaluateResult Evaluate();
    }
}
