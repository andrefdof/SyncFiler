using SyncFiler.Helpers;
using SyncFiler.Services;


HelloSquare.SayHello("Hello Veeam ");

var parseResult = CommandLineHelper.ParseAndProcessArguments(args);

await StartProgram.StartAsync(parseResult.Value, new CancellationToken());