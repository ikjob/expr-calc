using ExprCalc.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries.Models
{
    internal interface ICaluclationStatusDbModelView
    {
        Guid Id { get; }
        long UpdatedAt { get; }
        CalculationState State { get; }

        double? CalcResult { get; }
        CalculationErrorCode? ErrorCode { get; }
        CalculationErrorDetailsDbModel? ErrorDetails { get; }
        long? CancelledById { get; }

        UserDbModel? CancelledBy { get; }
    }
}
