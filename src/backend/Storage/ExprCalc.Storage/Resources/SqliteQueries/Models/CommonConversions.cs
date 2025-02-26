using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Storage.Resources.SqliteQueries.Models
{
    internal static class CommonConversions
    {
        public static long DateTimeToTimestamp(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                dateTime = dateTime.ToUniversalTime();

            return (long)(dateTime - DateTime.UnixEpoch).TotalMilliseconds;
        }
        public static DateTime TimestampToDateTime(long timestamp)
        {
            return DateTime.UnixEpoch + TimeSpan.FromMilliseconds(timestamp);
        }
    }
}
