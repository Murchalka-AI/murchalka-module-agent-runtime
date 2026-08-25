using Murchalka.AgentRuntime;
using Murchalka.AgentRuntime.Runtime;

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

await ModuleHost.RunAsync(new ModuleService(), shutdown.Token);

