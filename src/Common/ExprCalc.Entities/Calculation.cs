namespace ExprCalc.Entities
{
    /// <summary>
    /// Represent a single calculation with attached expression
    /// </summary>
    public class Calculation
    {
        public const int MaxExpressionLength = 25000;

        public static Calculation CreateUninitialized(string expression, User createdBy)
        {
            return new Calculation(Guid.Empty, expression, createdBy, default, status: null);
        }

        public Calculation(Guid id, string expression, User createdBy, DateTime createdAt, CalculationStatus? status)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentException("Expression cannot be empty", nameof(expression));
            if (expression.Length > MaxExpressionLength)
                throw new ArgumentException($"Expression length is large than allowed, MaxLength = {MaxExpressionLength}, Actual = {expression.Length}", nameof(expression));
            if (id != Guid.Empty && status == null)
                throw new ArgumentException("Status should be provided in initialized state");
            if (id == Guid.Empty && status != null)
                throw new ArgumentException("Status cannot be set in unitialized state");

            Id = id;
            Expression = expression;
            CreatedBy = createdBy;
            CreatedAt = createdAt;

            Status = status;
        }

        public Calculation(Calculation source)
        {
            Id = source.Id;
            Expression = source.Expression;
            CreatedBy = source.CreatedBy;
            CreatedAt = source.CreatedAt;

            Status = source.Status;
        }

        public Guid Id { get; private set; }
        public string Expression { get; }
        public User CreatedBy { get; }
        public DateTime CreatedAt { get; private set; }

        public CalculationStatus? Status { get; private set; }


        public bool IsInitialized => Id != Guid.Empty;

        public void Initialize(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("In initialized state Id cannot be equal to empty guid", nameof(id));

            Id = id;
            CreatedAt = DateTime.UtcNow;
            Status = CalculationStatus.CreatePending();
        }


        public Calculation DeepClone()
        {
            var result = new Calculation(this);
            if (Status != null)
                result.Status = new CalculationStatus(Status);

            return result;
        }
        public override string ToString()
        {
            return $"[Id = {Id}, Expression = '{Expression}']";
        }
    }
}
