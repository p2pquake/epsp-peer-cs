using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.App.Userquake
{
    struct Userquake
    {
        public DateTime At { get; set; }
        public string AreaCode { get; set; }
    }

    public class Aggregator
    {
        public event EventHandler<UserquakeEvaluateEventArgs> OnNew;
        public event EventHandler<UserquakeEvaluateEventArgs> OnUpdate;
        public IUserquakeEvaluation Evaluation { get; private set; }
        public bool IsDetecting { get; private set; }

        ICollection<Userquake> userquakes;

        public IUserquakeEvaluation EvaluationAt(DateTime at)
        {
            return UserquakeIsOver(at) ? null : Evaluation;
        }

        public bool IsDetectingAt(DateTime at)
        {
            return IsDetecting && !UserquakeIsOver(at);
        }

        public void AddUserquake(DateTime at, string areaCode, IReadOnlyDictionary<string, int> areaPeers)
        {
            // リセット、追加
            if (UserquakeIsOver(at))
            {
                userquakes = new LinkedList<Userquake>();
                IsDetecting = false;
                Evaluation = null;
            }
            userquakes.Add(new Userquake() { At = at, AreaCode = areaCode });

            // 評価
            var evaluation = Evaluate(userquakes, areaPeers);
            Evaluation = evaluation;
            if (evaluation == null || evaluation.ConfidenceLevel < 3)
            {
                return;
            }

            // 更新
            var e = new UserquakeEvaluateEventArgs()
            {
                StartedAt = evaluation.StartedAt,
                UpdatedAt = evaluation.UpdatedAt,
                Count = evaluation.Count,
                Confidence = evaluation.Confidence,
                AreaConfidences = evaluation.AreaConfidences,
            };

            if (!IsDetecting)
            {
                IsDetecting = true;
                OnNew(this, e);
            }
            else
            {
                OnUpdate(this, e);
            }
        }

        IUserquakeEvaluation Evaluate(ICollection<Userquake> userquakes, IReadOnlyDictionary<string, int> areaPeers)
        {
            var confidence = Enumerable.Range(1, Math.Max(userquakes.Count - 2, 1)).Max(c =>
            {
                var partialUserquakes = userquakes.Take(c);
                return CalcConfidence(partialUserquakes, areaPeers);
            });

            var areaConfidences = CalcAreaConfidences(userquakes, areaPeers);

            return new UserquakeEvaluateEventArgs()
            {
                StartedAt = userquakes.First().At,
                UpdatedAt = userquakes.Last().At,
                Count = userquakes.Count(),
                Confidence = confidence,
                AreaConfidences = areaConfidences,
            };
        }

        private bool UserquakeIsOver(DateTime at)
        {
            return userquakes == null || at.Subtract(userquakes.Last().At).TotalSeconds > 40;
        }

        private double CalcConfidence(IEnumerable<Userquake> userquakes, IReadOnlyDictionary<string, int> areaPeers)
        {
            if (userquakes.Count() < 3)
            {
                return 0.0;
            }

            var speed = userquakes.Count() / userquakes.Last().At.Subtract(userquakes.First().At).TotalSeconds;
            var rate = (double)userquakes.Count() / areaPeers.Values.Sum();
            var areaRate = CalcMaxAreaRate(userquakes, areaPeers);
            var regionRate = CalcMaxRegionRate(userquakes, areaPeers);

            var factor = 1.2;
            var confidence = 0.97024;

            if (speed >= 0.25 * factor && areaRate >= 0.05 * factor)
            {
                return confidence;
            }
            if (speed >= 0.15 * factor && areaRate >= 0.3 * factor)
            {
                return confidence;
            }
            if (rate >= 0.01 * factor && areaRate >= 0.035 * factor)
            {
                return confidence;
            }
            if (rate >= 0.006 * factor && areaRate >= 0.04 * factor && regionRate >= new double[] { 1 * factor, 1.0 }.Min())
            {
                return confidence;
            }
            if (speed >= 0.18 * factor && areaRate >= 0.04 * factor && regionRate >= new double[] { 1 * factor, 1.0 }.Min())
            {
                return confidence;
            }

            return 0.0;
        }

        private double CalcMaxAreaRate(IEnumerable<Userquake> userquakes, IReadOnlyDictionary<string, int> areaPeers)
        {
            var uqByArea = userquakes.GroupBy(uq => uq.AreaCode).ToDictionary(group => group.Key, group => group.Count());
            return uqByArea.Max(uq =>
            {
                if (uq.Key[0] == '9' || !areaPeers.ContainsKey(uq.Key))
                {
                    return 0;
                }

                return (double)uq.Value / areaPeers[uq.Key];
            });
        }

        private double CalcMaxRegionRate(IEnumerable<Userquake> userquakes, IReadOnlyDictionary<string, int> areaPeers)
        {
            var uqByRegion = userquakes.GroupBy(uq => uq.AreaCode[0]).ToDictionary(group => group.Key, group => group.Count());

            return uqByRegion.Max(uq =>
            {
                if (uq.Key == '9')
                {
                    return 0;
                }

                return (double)uq.Value / userquakes.Count();
            });
        }

        private IReadOnlyDictionary<string, IUserquakeEvaluationArea> CalcAreaConfidences(IEnumerable<Userquake> userquakes, IReadOnlyDictionary<string, int> areaPeers)
        {
            Dictionary<string, IUserquakeEvaluationArea> result = new();

            if (userquakes.Count() < 3)
            {
                return result;
            }

            // 先頭 2 件はかならず表示対象
            foreach (var userquake in userquakes.Take(2))
            {
                result[userquake.AreaCode] = new UserquakeEvaluationArea() { Confidence = 0 };
            }

            // 表示判定
            foreach (var i in Enumerable.Range(3, userquakes.Count() - 2))
            {
                var partialUserquakes = userquakes.Take(i);

                var xs = result.Select(e => AreaPositions.Items[e.Key].X);
                var ys = result.Select(e => AreaPositions.Items[e.Key].Y);

                // 近接による
                if (AreaPositions.Items.ContainsKey(partialUserquakes.Last().AreaCode))
                {
                    var p = AreaPositions.Items[partialUserquakes.Last().AreaCode];
                    if (xs.Min()-35 <= p.X && p.X <= xs.Max()+35 &&
                        ys.Min()-45 <= p.Y && p.Y <= ys.Max() + 45)
                    {
                        result[partialUserquakes.Last().AreaCode] = new UserquakeEvaluationArea() { Confidence = 0 };
                    }
                }

                // 発信数による
                var uqByArea = userquakes.GroupBy(uq => uq.AreaCode).ToDictionary(group => group.Key, group => group.Count());
                foreach (var (areaCode, count) in uqByArea)
                {
                    if (!AreaPositions.Items.ContainsKey(areaCode))
                    {
                        continue;
                    }
                    if (result.ContainsKey(areaCode))
                    {
                        continue;
                    }

                    if ((count >= 3 && (double)count / areaPeers[areaCode] >= 0.5) ||
                        (count >= 5 && (double)count / areaPeers[areaCode] >= 0.1))
                    {
                        result[areaCode] = new UserquakeEvaluationArea() { Confidence = 0 };
                    }
                }
            }

            // 信頼度決定・値設定
            foreach (var i in Enumerable.Range(3, userquakes.Count() - 2))
            {
                var uqByArea = userquakes.GroupBy(uq => uq.AreaCode).ToDictionary(group => group.Key, group => group.Count());
                var uqByPref = userquakes.GroupBy(uq => uq.AreaCode[0..2]).ToDictionary(group => group.Key, group => group.Count());
                var uqByRegion = userquakes.GroupBy(uq => uq.AreaCode[0]).ToDictionary(group => group.Key, group => group.Count());

                var peerByPref = areaPeers.GroupBy(p => p.Key[0..2]).ToDictionary(g => g.Key, g => g.Sum(p => p.Value));
                var peerByRegion = areaPeers.GroupBy(p => p.Key[0]).ToDictionary(g => g.Key, g => g.Sum(p => p.Value));
                
                foreach (var uqArea in uqByArea)
                {
                    if (!areaPeers.ContainsKey(uqArea.Key))
                    {
                        continue;
                    }

                    if (!result.ContainsKey(uqArea.Key))
                    {
                        continue;
                    }

                    var c = (double)uqArea.Value / areaPeers[uqArea.Key] * 100;
                    if ((double)uqArea.Value / areaPeers.Sum(a => a.Value) < 0.01)
                    {
                        c *= (double)uqArea.Value / areaPeers.Sum(a => a.Value) * 100;
                    } else
                    {
                        c *= 1.2;
                    }

                    c *= (double)uqByPref[uqArea.Key[0..2]] / peerByPref[uqArea.Key[0..2]] * 5 + 1;
                    c *= (double)uqByRegion[uqArea.Key[0]] / peerByRegion[uqArea.Key[0]] * 5 + 1;
                    c = Math.Max(0, Math.Min(c, 100.0));

                    result[uqArea.Key] = new UserquakeEvaluationArea() { AreaCode = uqArea.Key, Count = uqArea.Value, Confidence = c };
                }
            }

            foreach (var uqArea in userquakes.GroupBy(uq => uq.AreaCode).ToDictionary(group => group.Key, group => group.Count()))
            {
                if (!result.ContainsKey(uqArea.Key))
                {
                    result[uqArea.Key] = new UserquakeEvaluationArea() { AreaCode = uqArea.Key, Count = uqArea.Value, Confidence = -1 };
                }
            }

            return result;
        }
    }
}
