# DiscoSdk

A Discord SDK for .NET built with C#, featuring a Fluent API design and a REST Action pattern for deferred execution. This project serves as an architectural study, exploring clean architecture principles and modern C# design patterns.

## Overview

DiscoSdk is a comprehensive Discord bot framework written in C# (.NET 8.0) that provides a type-safe, fluent interface for interacting with the Discord API. The SDK is designed with a focus on clean architecture, separation of concerns, and developer experience.

## Architecture

The project is divided into two main components:

### DiscoSdk (Core SDK)
The core SDK library that defines interfaces, models, and contracts. This layer contains:
- **Models**: Discord entities (Users, Guilds, Channels, Messages, etc.)
- **Interfaces**: Contracts for clients, events, and REST actions
- **Enums**: Type-safe enumerations for Discord API values
- **REST Actions**: Interface definitions for deferred API operations

### DiscoSdk.Hosting
The hosting implementation that provides concrete implementations of the SDK interfaces. This layer includes:
- **DiscordClient**: Main client implementation for Gateway and REST API
- **Gateway**: WebSocket connection management and sharding
- **REST Clients**: HTTP client implementations for Discord API
- **Event System**: Event dispatching and processing
- **Wrappers**: Concrete implementations of Discord entities

## Key Features

### Fluent API Design
The SDK uses a fluent API pattern throughout, allowing for intuitive and chainable method calls:

```csharp
await channel.SendMessage()
    .SetContent("Hello, Discord!")
    .AddEmbeds(embed)
    .AddActionRow(button)
    .ExecuteAsync();
```

### REST Action Pattern
All server-side operations return `IRestAction<T>` or `IRestAction`, enabling deferred execution and better control over when API calls are made:

```csharp
// Action is created but not executed yet
var action = channel.SendMessage()
    .SetContent("Message content");

// Execute when ready
await action.ExecuteAsync();
```

This pattern provides:
- **Deferred Execution**: Build complex operations before executing
- **Composability**: Chain multiple operations together
- **Flexibility**: Execute synchronously or asynchronously as needed

### Type Safety
The SDK emphasizes type safety with:
- Strongly-typed Discord IDs (`DiscordId` instead of `ulong` or `string`)
- Enum-based types for all Discord API constants
- Generic type parameters for channel and entity types
- Nullable reference types throughout

### Event System
A robust event system for handling Discord Gateway events:

```csharp
client.EventRegistry.Register<MessageCreateEvent>(async (event) => {
    // Handle message creation
});
```

## Project Structure

```
DiscoSdk/
├── Commands/          # Application command interfaces
├── Events/            # Event system interfaces
├── Exceptions/        # Custom exception types
├── Logging/           # Logging abstractions
├── Models/            # Discord entity models
│   ├── Channels/      # Channel type models
│   ├── Commands/      # Command models
│   ├── Enums/         # Type-safe enumerations
│   ├── Messages/      # Message-related models
│   └── ...
├── Rest/
│   └── Actions/       # REST action interfaces
└── Utils/             # Utility classes

DiscoSdk.Hosting/
├── Builders/          # Fluent builders
├── Events/            # Event implementation
├── Gateway/           # WebSocket Gateway implementation
├── Logging/           # Logging implementations
├── Rest/
│   ├── Actions/       # REST action implementations
│   └── Clients/       # HTTP client implementations
└── Wrappers/          # Entity wrapper implementations
```

## Requirements

- .NET 8.0 or later
- Discord Bot Token

## Usage Example

```csharp
var config = new DiscordClientConfig
{
    Token = "your-bot-token",
    Intents = DiscordIntent.GuildMessages | DiscordIntent.MessageContent
};

var client = new DiscordClient(config);

// Register event handlers
client.EventRegistry.Register<MessageCreateEvent>(async (e) =>
{
    if (e.Message.Content == "!hello")
    {
        await e.Message.Channel.SendMessage()
            .SetContent("Hello!")
            .ExecuteAsync();
    }
});

await client.StartAsync();
```

## Design Principles

This project serves as an **architectural study**, exploring:

- **Clean Architecture**: Separation between core SDK and hosting implementation
- **Dependency Inversion**: Interfaces defined in core, implementations in hosting
- **Fluent API Patterns**: Builder and method chaining for improved developer experience
- **Deferred Execution**: REST Action pattern for flexible API interaction
- **Type Safety**: Strong typing throughout to prevent runtime errors
- **Extensibility**: Plugin-like architecture for events and commands