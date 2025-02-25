using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.Entities.MetadataParams
{
    public record struct PaginationParams(uint Offset, uint Limit, bool IncludeCount = false)
    {
        public static PaginationParams AllData => new PaginationParams(0, uint.MaxValue, false);

        public static PaginationParams FromPageNumberAndSize(uint pageNumber, uint pageSize, bool includeCount = false)
        {
            if (pageNumber > 0)
                pageNumber = pageNumber - 1;

            ulong offset = (ulong)pageNumber * pageSize;
            if (offset > uint.MaxValue)
                offset = uint.MaxValue;

            return new PaginationParams((uint)offset, pageSize, includeCount);
        }

    }
}
