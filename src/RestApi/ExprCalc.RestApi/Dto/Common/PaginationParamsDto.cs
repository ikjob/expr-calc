using ExprCalc.Entities.MetadataParams;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Dto.Common
{
    public class PaginationParamsDto
    {
        public const uint DefaultPageNumber = 0;
        public const uint DefaultPageSize = 30;

        [FromQuery]
        public uint? PageNumber { get; init; }
        [FromQuery]
        public uint? PageSize { get; init; }

        public PaginationParams IntoEntity(bool includeCount = true)
        {
            return PaginationParams.FromPageNumberAndSize(PageNumber ?? DefaultPageNumber, PageSize ?? DefaultPageSize, includeCount);
        }
    }
}
