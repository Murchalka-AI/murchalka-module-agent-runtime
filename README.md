# Agent Runtime

Phase 5 first-party module `dev.murchalka.agent-runtime`.

The repository is independently buildable and released as a signed immutable Module Bundle. Its public wire contract is `agent.turn@1`; stateful operations use the bound `storage.records` provider.

## Build

```bash
dotnet restore Murchalka.AgentRuntime.slnx --configfile NuGet.Config
dotnet build Murchalka.AgentRuntime.slnx --configuration Release --no-restore
dotnet test --solution Murchalka.AgentRuntime.slnx --configuration Release --no-build --no-restore
```

Tags in canonical `vX.Y.Z` form run validation and publish immutable release artifacts.

