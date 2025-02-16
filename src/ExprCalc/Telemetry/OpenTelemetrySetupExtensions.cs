using ExprCalc.Common.Instrumentation;
using ExprCalc.CoreLogic;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ExprCalc.Telemetry
{
    internal static class OpenTelemetrySetupExtensions
    {
        public static void SetupOpenTelemetry(this IHostApplicationBuilder hostApplicationBuilder)
        {
            var otelBuilder = hostApplicationBuilder.Services.AddOpenTelemetry();
            SetupMetrics(otelBuilder, hostApplicationBuilder.Configuration);
            SetupTracing(otelBuilder, hostApplicationBuilder.Configuration);
        }

        private static void SetupMetrics(OpenTelemetryBuilder otelBuilder, IConfigurationManager configuration)
        {
            var metricsConfig = new MetricsConfig();
            configuration.Bind(MetricsConfig.ConfigurationSectionName, metricsConfig);

            if (metricsConfig.Enable)
            {
                otelBuilder.WithMetrics(omBuilder =>
                {
                    omBuilder.AddPrometheusExporter(exporterOpts =>
                    {
                        exporterOpts.ScrapeEndpointPath = metricsConfig.RelativeUri;
                    });

                    omBuilder.AddAspNetCoreInstrumentation();

                    var registry = new MetricsRegistry();
                    registry.AddCoreLogicMetrics();

                    omBuilder.AddMeter(registry.MetricNames.ToArray());
                });
            }
        }

        private static void SetupTracing(OpenTelemetryBuilder otelBuilder, IConfigurationManager configuration)
        {
            var tracingConfig = new TracingConfig();
            configuration.Bind(TracingConfig.ConfigurationSectionName, tracingConfig);

            if (tracingConfig.IsEnable)
            {
                otelBuilder.WithTracing(tBuilder =>
                {
                    tBuilder.AddAspNetCoreInstrumentation();
                    tBuilder.AddHttpClientInstrumentation();

                    var registry = new ActivitySourcesRegistry();
                    registry.AddCoreLogicActivitySources();

                    tBuilder.AddSource(registry.ActivitySourceNames.ToArray());


                    if (tracingConfig.EnableConsoleExporter)
                        tBuilder.AddConsoleExporter();

                    if (tracingConfig.OtlpEndpoint != null)
                    {
                        tBuilder.AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri(tracingConfig.OtlpEndpoint);
                        });
                    }
                });

                otelBuilder.ConfigureResource(rBuilder =>
                {
                    rBuilder.AddService(tracingConfig.ServiceName);
                });
            }
        }


        public static void UseOpenTelemetryFeatures(this WebApplication app)
        {
            var metricsConfig = new MetricsConfig();
            app.Configuration.Bind(MetricsConfig.ConfigurationSectionName, metricsConfig);

            if (metricsConfig.Enable)
            {
                app.MapPrometheusScrapingEndpoint();
            }
        }
    }
}
