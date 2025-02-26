using ExprCalc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries.Models
{
    internal class UserDbModel
    {
        public required long Id { get; set; }
        public required string Login { get; set; }

        public UserDbModel Clone()
        {
            return new UserDbModel()
            {
                Id = Id,
                Login = Login
            };
        }

        public static UserDbModel FromEntity(User user, long id = 0)
        {
            return new UserDbModel()
            {
                Id = id,
                Login = user.Login
            };
        }

        public User IntoEntity()
        {
            return new User(Login);
        }
    }
}
