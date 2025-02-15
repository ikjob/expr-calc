namespace ExprCalc.Telemetry
{
    public class MetricsConfig
    {
        public const string ConfigurationSectionName = "Metrics";

        public string RelativeUri { get; set; } = "/metrics";
        public bool Enable { get; set; }
    }
}
