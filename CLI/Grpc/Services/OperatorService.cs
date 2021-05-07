using Client.App;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLI.Grpc.Services
{
    public class OperatorService : Operator.OperatorBase
    {
        private readonly MediatorContext _m;

        public OperatorService(MediatorContext mediatorContext)
        {
            _m = mediatorContext;
        }

        public override Task<BoolResponse> Connect(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new BoolResponse { Result = _m.Connect() });
        }

        public override Task<BoolResponse> Disconnect(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new BoolResponse { Result = _m.Disconnect() });
        }

        public override Task<BoolResponse> SendUserquake(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new BoolResponse { Result = _m.SendUserquake() });
        }

        public override Task<GetStateResponse> GetState(GetStateRequest request, ServerCallContext context)
        {
            State state = new()
            {
                AppState = _m.State.GetType().Name switch
                {
                    "DisconnectedState" => State.Types.AppState.Disconnected,
                    "ConnectingState" => State.Types.AppState.Connecting,
                    "ConnectedState" => State.Types.AppState.Connected,
                    "DisconnectingState" => State.Types.AppState.Disconnecting,
                    "MaintenanceState" => State.Types.AppState.Maintenance,
                    _ => State.Types.AppState.Unknown,
                },
                Connections = _m.Connections,
                IsPortOpened = _m.IsPortOpened,
                PeerId = _m.PeerId,
                HasKey = _m.Key != null,
                CanConnect = _m.CanConnect,
                CanDisconnect = _m.CanDisconnect,
            };
            _m.AreaPeerDictionary.ToList().ForEach(e => state.Areapeers.Add(e.Key, e.Value));

            return Task.FromResult(new GetStateResponse { State = state });
        }

        public override Task<GetConfigurationResponse> GetConfiguration(GetConfigurationRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetConfigurationResponse
            {
                Config = new ApplicationConfig
                {
                    AreaCode = _m.AreaCode,
                    PortOpen = _m.IsPortOpen,
                    Port = _m.Port,
                    UseUpnp = _m.UseUPnP,
                    MaxConnections = _m.MaxConnections,
                },
            });
        }

        public override Task<SetConfigurationResponse> SetConfiguration(SetConfigurationRequest request, ServerCallContext context)
        {
            _m.AreaCode = request.Config.AreaCode;
            _m.IsPortOpen = request.Config.PortOpen;
            _m.Port = request.Config.Port;
            _m.UseUPnP = request.Config.UseUpnp;
            _m.MaxConnections = request.Config.MaxConnections;

            return Task.FromResult(new SetConfigurationResponse { Result = true });
        }
    }
}
