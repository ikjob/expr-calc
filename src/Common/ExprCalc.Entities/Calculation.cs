namespace ExprCalc.Entities
{
    /// <summary>
    /// Represent single calculation with attached expression
    /// </summary>
    public record class Calculation
    {
        public required Guid Id { get; set; }
        public required string Expression { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
