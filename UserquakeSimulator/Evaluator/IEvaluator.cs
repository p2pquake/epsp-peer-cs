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
        double Accuracy { get; set; }
    }

    public interface IEvaluator
    {
        IReader Reader { set; }
        IUQManager UQManager { set; }
        IUQJudge UQJudge { set; }
        IVerifier Verifier { set; }

        EvaluateResult Evaluate();
    }
}
