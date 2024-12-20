using AvaloniaUIClient.ViewModels;

using Client.App;
using Client.App.Userquake;
using Client.Peer;

using JsonApi;

using Map.Model;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaUIClient.Mediator
{
    public class HistoryReloader
    {
        private readonly InformationViewModel informationViewModel;
        private readonly MediatorContext mediatorContext;

        private DateTime latestConnectedTime = DateTime.MinValue;

        private const int historyLimit = 100;

        public HistoryReloader(InformationViewModel informationViewModel, MediatorContext mediatorContext)
        {
            this.informationViewModel = informationViewModel;
            this.mediatorContext = mediatorContext;

            this.mediatorContext.ConnectionsChanged += MediatorContext_ConnectionsChanged;
        }

        private void MediatorContext_ConnectionsChanged(object? sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        // FIXME: あとで private にする
        public async void ReloadByApi()
        {
            var getTasks = Enumerable.Range(0, 5).Select(i => JsonApi.Client.Get(100, i * 100, Code.Earthquake, Code.Tsunami, Code.EEW, Code.UserquakeEvaluation));
            var results = await Task.WhenAll(getTasks);
            var items = results.SelectMany(e => e).Reverse();

            var events = new List<EventArgs>();
            foreach (var item in items)
            {
                // FIXME: あとで WpfClient/Program.cs と共通化する。
                var eventArgs = ConvertApiItemToEventArgs(item);
                if (eventArgs == null) { continue; }

                if (eventArgs is UserquakeEvaluateEventArgs userquakeEvaluateEventArgs)
                {
                    var index = events.FindIndex(e => e is UserquakeEvaluateEventArgs ue && ue.StartedAt == userquakeEvaluateEventArgs.StartedAt);
                    if (index >= 0)
                    {
                        events[index] = eventArgs;
                        continue;
                    }
                    events.Add(eventArgs);
                }
                else
                {
                    events.Add(eventArgs);
                }
            }

            informationViewModel.Histories.Clear();
            foreach (var e in events.Take(100).Reverse())
            {
                informationViewModel.Histories.Add(e);
            }
        }

        // FIXME: あとで WpfClient/Program.cs と共通化する。
        private static EventArgs? ConvertApiItemToEventArgs(BasicData item)
        {
            if (item is JMAQuake quake)
            {
                var eventArgs = new EPSPQuakeEventArgs()
                {
                    InformationType = quake.Issue.Type switch
                    {
                        "ScalePrompt" => QuakeInformationType.ScalePrompt,
                        "Destination" => QuakeInformationType.Destination,
                        "ScaleAndDestination" => QuakeInformationType.ScaleAndDestination,
                        "DetailScale" => QuakeInformationType.Detail,
                        "Foreign" => QuakeInformationType.Foreign,
                        _ => QuakeInformationType.Unknown,
                    },
                    TsunamiType = quake.Earthquake.DomesticTsunami switch
                    {
                        "None" => DomesticTsunamiType.None,
                        "Checking" => DomesticTsunamiType.Checking,
                        "NonEffective" => DomesticTsunamiType.None,
                        "Watch" => DomesticTsunamiType.Effective,
                        "Warning" => DomesticTsunamiType.Effective,
                        _ => DomesticTsunamiType.Unknown,
                    },
                    Depth = quake.Earthquake.Hypocenter.Depth == 0 ? "ごく浅い" : quake.Earthquake.Hypocenter.Depth == -1 ? "不明" : $"{quake.Earthquake.Hypocenter.Depth}km",
                    Destination = quake.Earthquake.Hypocenter.Name,
                    Magnitude = $"{quake.Earthquake.Hypocenter.Magnitude.ToString(NumberFormatInfo.InvariantInfo)}",
                    OccuredTime = DateTime.Parse(quake.Earthquake.Time).ToString("d日HH時mm分"),
                    Scale = ConvertScale(quake.Earthquake.MaxScale),
                    Latitude = quake.Earthquake.Hypocenter.Latitude <= -200 ? "" : $"{(quake.Earthquake.Hypocenter.Latitude > 0 ? 'N' : 'S')}{Math.Abs(quake.Earthquake.Hypocenter.Latitude).ToString(NumberFormatInfo.InvariantInfo)}",
                    Longitude = quake.Earthquake.Hypocenter.Longitude <= -200 ? "" : $"{(quake.Earthquake.Hypocenter.Longitude > 0 ? 'E' : 'W')}{Math.Abs(quake.Earthquake.Hypocenter.Longitude).ToString(NumberFormatInfo.InvariantInfo)}",
                    PointList = quake.Points.Select(e =>
                        new QuakeObservationPoint() { Prefecture = e.Pref, Name = e.Addr, Scale = ConvertScale(e.Scale) }
                    ).ToList(),
                };
                if (eventArgs.InformationType == QuakeInformationType.ScalePrompt)
                {
                    eventArgs.Depth = "";
                    eventArgs.Magnitude = "";
                }
                if (eventArgs.InformationType == QuakeInformationType.Destination)
                {
                    eventArgs.Scale = "3以上";
                }

                if (eventArgs.InformationType == QuakeInformationType.Unknown) { return null; }
                return eventArgs;
            }

            if (item is JMATsunami tsunami)
            {
                var eventArgs = new EPSPTsunamiEventArgs()
                {
                    IsCancelled = tsunami.Cancelled,
                    ReceivedAt = DateTime.Parse(tsunami.Time),
                    RegionList = tsunami.Areas.Select(e => new TsunamiForecastRegion()
                    {
                        IsImmediately = e.Immediate,
                        Category = e.Grade switch
                        {
                            "MajorWarning" => Client.Peer.TsunamiCategory.MajorWarning,
                            "Warning" => Client.Peer.TsunamiCategory.Warning,
                            "Watch" => Client.Peer.TsunamiCategory.Advisory,
                            _ => Client.Peer.TsunamiCategory.Unknown,
                        },
                        Region = e.Name
                    }).ToList(),
                };

                return eventArgs;
            }

            if (item is EEW eew)
            {
                var serial = 1;
                int.TryParse(eew.Issue.Serial, out serial);

                var eventArgs = new EPSPEEWEventArgs()
                {
                    IsTest = false,
                    IsCancelled = eew.Cancelled,
                    IsFollowUp = serial > 1,
                    ReceivedAt = DateTime.Parse(eew.Time),
                    Hypocenter = EEWConverter.GetHypocenterCode(eew.Earthquake?.Hypocenter?.ReduceName),
                    Areas = (eew.Areas ?? Array.Empty<EEWArea>()).Select(e => e.Pref).Distinct().Select(e => EEWConverter.GetAreaCode(e)).ToArray(),
                };

                return eventArgs;
            }

            if (item is UserquakeEvaluation evaluation)
            {
                if (evaluation.Confidence <= 0) { return null; }

                var eventArgs = new UserquakeEvaluateEventArgs()
                {
                    // XXX: JSON deserializer 側で DateTime にしておいてほしいかも
                    StartedAt = DateTime.Parse(evaluation.StartedAt),
                    UpdatedAt = DateTime.Parse(evaluation.UpdatedAt),
                    Count = evaluation.Count,
                    Confidence = evaluation.Confidence,
                    AreaConfidences = evaluation.AreaConfidences.Where(e => e.Value.Confidence >= 0).ToDictionary(e => e.Key.PadLeft(3, '0'), e => new UserquakeEvaluationArea() { AreaCode = e.Key.PadLeft(3, '0'), Confidence = e.Value.Confidence, Count = e.Value.Count } as IUserquakeEvaluationArea)
                };

                return eventArgs;
            }

            return null;
        }
        private static string ConvertScale(int scale)
        {
            return scale switch
            {
                10 => "1",
                20 => "2",
                30 => "3",
                40 => "4",
                45 => "5弱",
                50 => "5強",
                55 => "6弱",
                60 => "6強",
                70 => "7",
                46 => "5弱以上（推定）",
                _ => "不明",
            };
        }
    }
}
