# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is the P2PQuake Earthquake Software Platform (EPSP) - a peer-to-peer earthquake early warning system that distributes real-time seismic information across Japan. The system includes multiple client applications and a comprehensive P2P networking infrastructure.

## Build Commands

```bash
# Build entire solution
dotnet build Client.sln

# Build specific configurations
dotnet build Client.sln --configuration Release
dotnet build Client.sln --configuration DebugExcludeWPF

# Run tests
dotnet test ClientTest/ClientTest.csproj
dotnet test MapTest/MapTest.csproj
dotnet test PKCSPeerCryptoTest/PKCSPeerCryptoTest.csproj
dotnet test LegacyPluginSupporterTest/LegacyPluginSupporterTest.csproj

# Run applications
dotnet run --project AvaloniaUIClient/AvaloniaUIClient.csproj  # Cross-platform UI
dotnet run --project WpfClient/WpfClient.csproj              # Windows WPF UI
dotnet run --project CLI/CLI.csproj                          # Command line interface
dotnet run --project DummyPeer/DummyPeer.csproj             # Test peer for development
```

## Architecture

### Core Components

- **Client/** - Core P2P client library (.NET 6.0) with state machine-based connection management
- **MediatorContext** - Central orchestrator implementing the main application logic and event coordination
- **AvaloniaUIClient/** - Cross-platform UI client (.NET 8.0, Avalonia 11.1.0)
- **WpfClient/** - Windows-specific UI client (.NET 6.0, WPF with ModernWPF)
- **CLI/** - Command-line interface with gRPC server support
- **JsonApi/** - HTTP REST API client for P2PQuake web services
- **Map/** - Seismic data visualization and map rendering (ImageSharp-based)
- **PKCSPeerCrypto/** - Cryptographic verification for P2P network security

### Data Flow Architecture

```
P2P Network/Server → Client Core → MediatorContext → UI Applications
                                        ↓
                                   JsonApi ← HTTP API
                                        ↓
                                   Map Rendering → Visual Display
```

### Key Design Patterns

- **State Machine**: Client connections managed through AbstractState implementations
- **Event-Driven**: Real-time earthquake data distributed via C# events
- **MVVM**: UI applications use ViewModel pattern with CommunityToolkit.Mvvm
- **Observer Pattern**: Multiple observer implementations for different output formats
- **Mediator Pattern**: MediatorContext coordinates between components

## Development Guidelines

### Project Structure
- Each major component is a separate .NET project with clear dependencies
- UI clients reference Client, JsonApi, and Map libraries
- Test projects mirror the structure of their corresponding implementation projects
- Mixed target frameworks: .NET 6.0 for core libraries, .NET 8.0 for Avalonia UI

### Key Interfaces
- **IMediatorContext** - Core application orchestrator interface
- **IOperatable** - Connection management operations
- **IPeerState/IPeerConfig** - P2P network state and configuration
- **IObserver** - Data output abstraction (used in CLI)

### Event System
The system relies heavily on C# events for real-time data distribution:
- `OnEarthquake` - Earthquake information events
- `OnEEW` - Earthquake Early Warning events  
- `OnTsunami` - Tsunami warning events
- `OnUserquake` - User-reported earthquake events

### Testing Framework
- **NUnit 3.13.3** with .NET Test SDK 17.5.0
- **Moq 4.18.4** for mocking dependencies
- Test data files located in `TestData/` subdirectories
- Coverage collection via coverlet.collector

## Special Considerations

### Multi-Platform Support
- Avalonia UI client supports Windows, macOS, and Linux
- WPF client is Windows-only but provides native Windows integration
- Use conditional compilation for platform-specific features

### P2P Networking
- Custom P2P protocol implementation with state machine management
- UPnP port forwarding support via Mono.Nat
- Cryptographic peer verification using PKCS standards
- Automatic reconnection logic with exponential backoff

### Real-Time Requirements
- System designed for low-latency earthquake data distribution
- Event-driven architecture minimizes processing delays
- Sound notification system for immediate earthquake alerts
- Map rendering optimized for real-time updates

### Japanese Localization
- Primary language is Japanese - UI text and comments are in Japanese
- Earthquake intensity scales follow JMA (Japan Meteorological Agency) standards
- Geographic data focused on Japan's seismic monitoring network