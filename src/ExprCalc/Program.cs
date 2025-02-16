using ExprCalc.CommandLine;
using ExprCalc.CoreLogic;
using ExprCalc.Logging;
using ExprCalc.RestApi;
using ExprCalc.Telemetry;

namespace ExprCalc
{
    public class Program
    {
        private const string _appsettingsFilename = "appsettings.json";

        public static int Main(string[] args)
        {
            var parsedArgs = CommandLineArgumentsParser.Parse(args);
            if (parsedArgs == null)
                return -1;

            Serilog.Log.Logger = SerilogBootstrapLoggerFactory.Create(parsedArgs.ConfigPath ?? _appsettingsFilename);

            try
            {
                Serilog.Log.Information("Starting Application");
                CreateWebApplication(args, parsedArgs).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Unexpected exception");
                throw;
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }

        private static WebApplication CreateWebApplication(string[] args, CommandLineArguments parsedArgs)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (parsedArgs.ConfigPath != null)
                builder.Configuration.AddJsonFile(parsedArgs.ConfigPath, optional: false);


            builder.Host.SetupSerilog();
            builder.SetupOpenTelemetry();

            // Add services to the container.
            SetupServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            app.UseRestApiFeatures();
            app.UseOpenTelemetryFeatures();

            return app;
        }

        private static void SetupServices(IServiceCollection services, ConfigurationManager configurationManager)
        {
            services.AddRestApiServices(configurationManager);
            services.AddCoreLogicServices();
        }
    }
}
