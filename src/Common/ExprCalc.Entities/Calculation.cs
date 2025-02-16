namespace ExprCalc.Entities
{
    /// <summary>
    /// Represent a single calculation with attached expression
    /// </summary>
    public class Calculation
    {
        public static Calculation CreateUninitialized(string expression)
        {
            return new Calculation(Guid.Empty, expression, default);
        }

        public Calculation(Guid id, string expression, DateTime createdAt)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentException("Expression cannot be empty", nameof(expression));

            Id = id;
            Expression = expression;
            CreatedAt = createdAt;
        }

        public Calculation(Calculation source)
        {
            Id = source.Id;
            Expression = source.Expression;
            CreatedAt = source.CreatedAt;
        }

        public Guid Id { get; private set; }
        public string Expression { get; }
        public DateTime CreatedAt { get; private set; }


        public bool IsInitialized => Id != Guid.Empty;

        public void Initialize(Guid id, DateTime createdAt)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("In initialized state Id cannot be equal to empty guid", nameof(id));

            Id = id;
            CreatedAt = createdAt;
        }

        public override string ToString()
        {
            return $"[Id = {Id}, Expression = '{Expression}']";
        }
    }
}
