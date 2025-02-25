using ExprCalc.Entities.MetadataParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Dto.Common
{
    public class PaginatedResultDto<T>
    {
        public required IEnumerable<T> Data { get; set; }
        public required PaginationMetadataDto Metadata { get; set; }
    }

    public record struct PaginationMetadataDto
    {
        public required uint PageNumber { get; init; }
        public required uint PageSize { get; init; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public uint? TotalPagesCount { get; init; }

        public static PaginationMetadataDto FromEntity<T>(in PaginatedResult<T> entity)
        {
            return new PaginationMetadataDto()
            {
                PageNumber = entity.PageNumber,
                PageSize = entity.PageSize,
                TotalPagesCount = entity.TotalPagesCount
            };
        }
    }
}
