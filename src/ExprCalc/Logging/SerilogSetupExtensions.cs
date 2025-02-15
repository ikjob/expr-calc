using Serilog;

namespace ExprCalc.Logging
{
    internal static class SerilogSetupExtensions
    {
        internal static IHostBuilder SetupSerilog(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((hostingContext, configuration) =>
            {
                configuration.ReadFrom.Configuration(hostingContext.Configuration);
            });
        }
    }
}
