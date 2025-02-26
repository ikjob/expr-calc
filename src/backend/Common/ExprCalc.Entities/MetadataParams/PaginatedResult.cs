using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities.MetadataParams
{
    public readonly struct PaginatedResult<T>
    {
        public PaginatedResult(List<T> items, uint offset, uint limit, uint? totalItemsCount = null)
        {
            Items = items;
            Offset = offset;
            Limit = limit;
            TotalItemsCount = totalItemsCount;
        }

        public List<T> Items { get; init; }
        public uint Offset { get; init; }
        public uint Limit { get; init; }
        public uint? TotalItemsCount { get; init; }

        public uint PageNumber
        {
            get
            {
                if (Limit == 0)
                    return 0;
                return (Offset / Limit) + 1;
            }
        }
        public uint PageSize => Limit;
        public uint? TotalPagesCount
        {
            get
            {
                if (!TotalItemsCount.HasValue)
                    return null;
                if (Limit == 0)
                    return null;
                return (TotalItemsCount.Value / Limit) + ((TotalItemsCount % Limit > 0) ? 1u : 0u);
            }
        }
    }
}
