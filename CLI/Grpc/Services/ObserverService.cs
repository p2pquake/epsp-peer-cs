using Client.App;
using Client.App.Userquake;
using Client.Client;
using Client.Client.General;
using Client.Peer;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLI.Grpc.Services
{
    public class ObserverService : Observer.ObserverBase
    {
        private readonly MediatorContext _m;

        public ObserverService(MediatorContext mediatorContext)
        {
            _m = mediatorContext;
        }

        public override Task WatchData(Empty request, IServerStreamWriter<DataArrivedResponse> responseStream, ServerCallContext context)
        {
            void onEarthquake(object s, EPSPQuakeEventArgs e)
            {
                Quake quake = new()
                {
                    OccuredTime = e.OccuredTime,
                    Scale = e.Scale,
                    TsunamiType = e.TsunamiType switch
                    {
                        DomesticTsunamiType.None => Quake.Types.DomesticTsunamiType.None,
                        DomesticTsunamiType.Effective => Quake.Types.DomesticTsunamiType.Effective,
                        DomesticTsunamiType.Checking => Quake.Types.DomesticTsunamiType.Checking,
                        _ => Quake.Types.DomesticTsunamiType.UnknownTsunamiType,
                    },
                    InformationType = e.InformationType switch
                    {
                        QuakeInformationType.ScalePrompt => Quake.Types.QuakeInformationType.ScalePrompt,
                        QuakeInformationType.Destination => Quake.Types.QuakeInformationType.Destination,
                        QuakeInformationType.ScaleAndDestination => Quake.Types.QuakeInformationType.ScaleAndDestination,
                        QuakeInformationType.Detail => Quake.Types.QuakeInformationType.Detail,
                        QuakeInformationType.Foreign => Quake.Types.QuakeInformationType.Foreign,
                        _ => Quake.Types.QuakeInformationType.UnknownInformationType,
                    },
                    Destination = e.Destination,
                    Depth = e.Depth,
                    Magnitude = e.Magnitude,
                    IsCorrection = e.IsCorrection,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    IssueFrom = e.IssueFrom,
                };
                e.PointList?.ToList().ForEach(e =>
                {
                    quake.Points.Add(new QuakeObservationPoint
                    {
                        Prefecture = e.Prefecture,
                        Name = e.Name,
                        Scale = e.Scale,
                    });
                });

                responseStream.WriteAsync(new DataArrivedResponse
                {
                    Type = DataArrivedResponse.Types.EventType.Earthquake,
                    ReceivedAt = e.ReceivedAt.ToTimestamp(),
                    IsInvalidSignature = e.IsInvalidSignature,
                    IsExpired = e.IsExpired,
                    Quake = quake
                });
            }
            void onTsunami(object s, EPSPTsunamiEventArgs e)
            {
                Tsunami tsunami = new() { IsCancelled = e.IsCancelled };
                e.RegionList?.ToList().ForEach(e =>
                {
                    tsunami.RegionList.Add(new TsunamiForecastRegion
                    {
                        IsImmediately = e.IsImmediately,
                        Region = e.Region,
                        Category = e.Category switch
                        {
                            TsunamiCategory.Advisory => TsunamiForecastRegion.Types.TsunamiCategory.Advisory,
                            TsunamiCategory.Warning => TsunamiForecastRegion.Types.TsunamiCategory.Warning,
                            TsunamiCategory.MajorWarning => TsunamiForecastRegion.Types.TsunamiCategory.MajorWarning,
                            _ => TsunamiForecastRegion.Types.TsunamiCategory.Unknown,
                        }
                    });
                });

                responseStream.WriteAsync(new DataArrivedResponse
                {
                    Type = DataArrivedResponse.Types.EventType.Tsunami,
                    ReceivedAt = e.ReceivedAt.ToTimestamp(),
                    IsInvalidSignature = e.IsInvalidSignature,
                    IsExpired = e.IsExpired,
                    Tsunami = tsunami,
                });
            }
            void onAreapeers(object s, EventArgs e)
            {
                responseStream.WriteAsync(new DataArrivedResponse
                {
                    Type = DataArrivedResponse.Types.EventType.Areapeers,
                });
            }
            void onEEWTest(object s, EPSPEEWTestEventArgs e)
            {
                responseStream.WriteAsync(new DataArrivedResponse
                {
                    Type = DataArrivedResponse.Types.EventType.EewTest,
                    ReceivedAt = e.ReceivedAt.ToTimestamp(),
                    IsInvalidSignature = e.IsInvalidSignature,
                    IsExpired = e.IsExpired,
                    EewTest = new EEWTest
                    {
                        IsTest = e.IsTest,
                    },
                });
            }
            void onUserquake(object s, EPSPUserquakeEventArgs e)
            {
                responseStream.WriteAsync(new DataArrivedResponse
                {
                    Type = DataArrivedResponse.Types.EventType.Userquake,
                    ReceivedAt = e.ReceivedAt.ToTimestamp(),
                    IsInvalidSignature = e.IsInvalidSignature,
                    IsExpired = e.IsExpired,
                    Userquake = new Userquake
                    {
                        AreaCode = e.AreaCode,
                        PublicKey = e.PublicKey,
                    },
                });
            }
            UserquakeEvaluation genUserquakeEvaluation(UserquakeEvaluateEventArgs e)
            {
                UserquakeEvaluation ue = new()
                {
                    StartedAt = e.StartedAt.ToTimestamp(),
                    UpdatedAt = e.UpdatedAt.ToTimestamp(),
                    Count = e.Count,
                    Confidence = e.Confidence,
                    ConfidenceLevel = e.ConfidenceLevel,
                };
                e.AreaConfidences.ToList().ForEach(e =>
                {
                    ue.AreaConfidences.Add(e.Key, new UserquakeEvaluationArea
                    {
                        AreaCode = e.Value.AreaCode,
                        Confidence = e.Value.Confidence,
                        ConfidenceLevel = e.Value.ConfidenceLevel,
                        Count = e.Value.Count,
                    });
                });
                return ue;
            }
            void onNewUserquakeEvaluation(object s, UserquakeEvaluateEventArgs e)
            {
                responseStream.WriteAsync(new DataArrivedResponse
                {
                    Type = DataArrivedResponse.Types.EventType.NewUserquakeEvaluation,
                    ReceivedAt = e.UpdatedAt.ToTimestamp(),
                    UserquakeEvaluation = genUserquakeEvaluation(e),
                });
            }
            void onUpdateUserquakeEvaluation(object s, UserquakeEvaluateEventArgs e)
            {
                responseStream.WriteAsync(new DataArrivedResponse
                {
                    Type = DataArrivedResponse.Types.EventType.UpdateUserquakeEvaluation,
                    ReceivedAt = e.UpdatedAt.ToTimestamp(),
                    UserquakeEvaluation = genUserquakeEvaluation(e),
                });
            }


            return Task.Run(async () =>
            {
                _m.OnEarthquake += onEarthquake;
                _m.OnTsunami += onTsunami;
                _m.OnAreapeers += onAreapeers;
                _m.OnEEWTest += onEEWTest;
                _m.OnUserquake += onUserquake;
                _m.OnNewUserquakeEvaluation += onNewUserquakeEvaluation;
                _m.OnUpdateUserquakeEvaluation += onUpdateUserquakeEvaluation;

                while(true)
                {
                    await Task.Delay(1000);
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        _m.OnEarthquake -= onEarthquake;
                        _m.OnTsunami -= onTsunami;
                        _m.OnAreapeers -= onAreapeers;
                        _m.OnEEWTest -= onEEWTest;
                        _m.OnUserquake -= onUserquake;
                        _m.OnNewUserquakeEvaluation -= onNewUserquakeEvaluation;
                        _m.OnUpdateUserquakeEvaluation -= onUpdateUserquakeEvaluation;

                        context.CancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }, context.CancellationToken);
        }

        public override Task WatchState(Empty request, IServerStreamWriter<StateChangedResponse> responseStream, ServerCallContext context)
        {
            void stateChanged(object s, EventArgs e) {
                responseStream.WriteAsync(new StateChangedResponse
                {
                    Type = StateChangedResponse.Types.EventType.StateChanged,
                });
            }

            void completed(object s, OperationCompletedEventArgs e) {
                responseStream.WriteAsync(new StateChangedResponse
                {
                    Type = StateChangedResponse.Types.EventType.Completed,
                    ErrorCode = e.ErrorCode switch
                    {
                        ClientConst.ErrorCode.SUCCESSFUL => StateChangedResponse.Types.ErrorCode.NoError,
                        ClientConst.ErrorCode.CONNECTION_FAILED => StateChangedResponse.Types.ErrorCode.ConnectionFailed,
                        ClientConst.ErrorCode.RETURNED_ADDRESS_CHANGED => StateChangedResponse.Types.ErrorCode.ReturnedAddressChanged,
                        ClientConst.ErrorCode.RETURNED_DIFF_SPEC => StateChangedResponse.Types.ErrorCode.ReturnedDiffSpec,
                        ClientConst.ErrorCode.RETURNED_INCOMPATIBLE_CLIENT => StateChangedResponse.Types.ErrorCode.ReturnedIncompatibleClient,
                        ClientConst.ErrorCode.RETURNED_INCOMPATIBLE_SERVER => StateChangedResponse.Types.ErrorCode.ReturnedIncompatibleServer,
                        ClientConst.ErrorCode.RETURNED_INVALID_REQUEST => StateChangedResponse.Types.ErrorCode.ReturnedInvalidRequest,
                        ClientConst.ErrorCode.RETURNED_MAINTENANCE => StateChangedResponse.Types.ErrorCode.ReturnedMaintenance,
                        ClientConst.ErrorCode.RETURNED_UNKNOWN => StateChangedResponse.Types.ErrorCode.ReturnedUnknown,
                        ClientConst.ErrorCode.TIMED_OUT => StateChangedResponse.Types.ErrorCode.TimedOut,
                        _ => StateChangedResponse.Types.ErrorCode.UnknownError,
                    },
                    OperationResult = e.Result switch
                    {
                        ClientConst.OperationResult.Successful => StateChangedResponse.Types.OperationResult.Successful,
                        ClientConst.OperationResult.Retryable => StateChangedResponse.Types.OperationResult.Retryable,
                        ClientConst.OperationResult.Restartable => StateChangedResponse.Types.OperationResult.Restartable,
                        ClientConst.OperationResult.Fatal => StateChangedResponse.Types.OperationResult.Fatal,
                        _ => StateChangedResponse.Types.OperationResult.UnknownResult,
                    },
                }) ;
            }

            void connetionsChanged(object s, EventArgs e) {
                responseStream.WriteAsync(new StateChangedResponse
                {
                    Type = StateChangedResponse.Types.EventType.ConnectionsChanged,
                });
            }

            return Task.Run(async () =>
            {
                _m.StateChanged += stateChanged;
                _m.Completed += completed;
                _m.ConnectionsChanged += connetionsChanged;

                while(true)
                {
                    await Task.Delay(1000);
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        _m.StateChanged -= stateChanged;
                        _m.Completed -= completed;
                        _m.ConnectionsChanged -= connetionsChanged;

                        context.CancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }, context.CancellationToken);
        }
    }
}
