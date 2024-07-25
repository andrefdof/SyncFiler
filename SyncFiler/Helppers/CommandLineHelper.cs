using CommandLine;
using SyncFiler.Classes;

namespace SyncFiler.Helppers
{
    public class CommandLineHelper
    {
        public static ParserResult<Options> ParseAndProcessArguments(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed<Options>(errs =>
                {
                    HandleParseErrors(errs);
                });

            return parseResult;
        }

        private static void HandleParseErrors(IEnumerable<Error> errs)
        {
            errs.ToList().ForEach(err => Console.WriteLine(err.ToString()));
            Environment.Exit(1);
        }
    }
}
