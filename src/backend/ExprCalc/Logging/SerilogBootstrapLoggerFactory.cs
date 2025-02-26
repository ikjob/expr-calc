using Serilog.Extensions.Hosting;
using Serilog;

namespace ExprCalc.Logging
{
    internal static class SerilogBootstrapLoggerFactory
    {
        /// <summary>
        /// The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
        /// logger configured in <see cref="Extensions.HostBuilderExtensions.SetupSerilog(IHostBuilder)"/> below, 
        /// once configuration and dependency-injection have both been set up successfully.
        /// </summary>
        internal static ReloadableLogger Create(string configFilename)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFilename)
                .Build();

            return new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateBootstrapLogger();
        }
    }
}
