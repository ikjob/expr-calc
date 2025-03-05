namespace ExprCalc.Telemetry
{
    public class TracingConfig
    {
        public const string ConfigurationSectionName = "Tracing";

        public string ServiceName { get; set; } = "TemplateApp";
        public bool EnableConsoleExporter { get; set; }
        public string? OtlpEndpoint { get; set; }

        public bool IsEnable => EnableConsoleExporter || !string.IsNullOrEmpty(OtlpEndpoint);
    }
}
