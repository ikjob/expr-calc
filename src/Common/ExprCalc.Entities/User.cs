using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities
{
    /// <summary>
    /// User information
    /// </summary>
    /// <remarks>
    /// Right now it is a value type (wrapper around logic string).
    /// In future it can be converted into an entity
    /// </remarks>
    public readonly record struct User
    {
        public const int MaxLoginLength = 32;

        public User(string login)
        {
            if (login.Length == 0)
                throw new ArgumentException("User login cannot be empty", nameof(login));
            if (login.Length > MaxLoginLength)
                throw new ArgumentException($"User login cannot be longer than {MaxLoginLength}", nameof(login));

            Login = login;
        }

        public string Login { get; }
    }
}
