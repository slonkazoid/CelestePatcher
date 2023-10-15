using CommandLine;
using Serilog;

namespace CelestePatcher
{
    class CelestePatcher
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Enable verbose output.")]
            public bool Verbose { get; set; }

            [Option(shortName: 'o', longName: "output", Required = false, HelpText = "Output file.",
                MetaValue = "file")]
            public string? OutputFile { get; set; }

            [Option(shortName: 'y', longName: "yes", Required = false,
                HelpText = "Assume yes as the answer to all prompts.")]
            public bool Yes { get; set; }

            [Value(0, Default = "Celeste.exe", MetaName = "file", HelpText = "Celeste.exe file to patch.")]
            public required string File { get; set; }
        }

        private static bool AskYesNo(string prompt = "Continue?", bool defaultYes = true)
        {
            ConsoleKey key;
            do
            {
                // Print prompt and wait for key
                Console.Error.Write("\r{0} ({1}/{2}) ", prompt, defaultYes ? "y".ToUpper() : "y",
                    defaultYes ? "n" : "n".ToUpper());
                key = Console.ReadKey().Key;
            } while ( // Wait for one of Y, N, Enter or Spacebar
                     key != ConsoleKey.Y && key != ConsoleKey.N &&
                     key != ConsoleKey.Enter && key != ConsoleKey.Spacebar);

            Console.Error.WriteLine();
            return defaultYes && key != ConsoleKey.N // If it's default yes, return true for Y, Enter, and Spacebar
                   || !defaultYes && key == ConsoleKey.Y; // Else, return false for N, Enter, and Spacebar
        }

        private static void Main(string[] args)
        {
            new Parser(config => config.HelpWriter = Console.Out).ParseArguments<Options>(args)
                // ReSharper disable once VariableHidesOuterVariable
                .WithParsed(args =>
                {
                    LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                        .WriteTo.Console();

                    var log = (args.Verbose
                        ? loggerConfiguration.MinimumLevel.Verbose()
                        : loggerConfiguration.MinimumLevel.Information()).CreateLogger();

                    Patcher patcher;
                    try
                    {
                        patcher = new Patcher(args.File, log);
                    }
                    catch (Exception e)
                    {
                        if (e is FileNotFoundException or BadImageFormatException)
                        {
                            log.Error(e.Message);
                            Environment.Exit(1);
                            return;
                        }

                        log.Error($"Unexpected error while loading file: {e.Message}");
                        throw;
                    }

                    if (!patcher.IsCeleste())
                    {
                        log.Warning("Assembly is not Celeste.");
                        if (!args.Yes && !AskYesNo(defaultYes: false))
                        {
                            log.Information("Cancelling");
                            return;
                        }
                    }

                    if (!patcher.NeedsPatching())
                    {
                        log.Warning("Assembly does not have Steamworks.NET AssemblyRef.");
                        if (!args.Yes && !AskYesNo(defaultYes: true))
                        {
                            log.Information("Cancelling");
                            return;
                        }
                    }

                    try
                    {
                        patcher.Patch();
                        log.Information("Patched successfully");
                    }
                    catch (Exception e)
                    {
                        log.Error($"Could not patch executable: {e.Message}");
                        throw;
                    }

                    var savePath = args.OutputFile ?? $"{args.File}.patched";
                    log.Information($"Saving to {savePath}");
                    patcher.Save(savePath);
                });
        }
    }
}