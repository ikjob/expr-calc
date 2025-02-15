using CommandLine;
using System.Globalization;

namespace ExprCalc.CommandLine
{
    public class CommandLineArguments
    {
        [Option('c', "config", Default = null, Required = false, HelpText = "Path to the config file", MetaValue = "<path>")]
        public string? ConfigPath { get; set; }
    }


    internal static class CommandLineArgumentsParser
    {
        private static readonly Parser _commandLineParser = new(s =>
        {
            s.IgnoreUnknownArguments = true;
            s.AutoHelp = true;
            s.AutoVersion = true;
            s.ParsingCulture = CultureInfo.InvariantCulture;
            s.GetoptMode = false;
            s.EnableDashDash = false;
            s.AllowMultiInstance = false;
            s.CaseInsensitiveEnumValues = true;
            s.HelpWriter = TextWriter.Synchronized(Console.Out);
        });

        public static CommandLineArguments? Parse(string[] args)
        {
            var result = _commandLineParser.ParseArguments<CommandLineArguments>(args).Value;
            if (result == null)
                return null;

            if (result.ConfigPath != null && !Path.Exists(result.ConfigPath))
            {
                Console.WriteLine("Config file at specified path is not exist");
                return null;
            }

            return result;
        }
    }
}
