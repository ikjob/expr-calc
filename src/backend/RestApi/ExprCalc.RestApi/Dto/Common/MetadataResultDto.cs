using ExprCalc.Entities.MetadataParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Dto.Common
{
    public class MetadataResultDto<T>
    {
        public required IEnumerable<T> Data { get; set; }
        public required QueryResultMetadataDto Metadata { get; set; }
    }

    public readonly record struct QueryResultMetadataDto
    {
        public required uint PageNumber { get; init; }
        public required uint PageSize { get; init; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public uint? TotalItemsCount { get; init; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? TimeOnServer { get; init; }

        public static QueryResultMetadataDto FromPaginationWithTime<T>(in PaginatedResult<T> entity, DateTime timeOnServer)
        {
            return new QueryResultMetadataDto()
            {
                PageNumber = entity.PageNumber,
                PageSize = entity.PageSize,
                TotalItemsCount = entity.TotalItemsCount,
                TimeOnServer = timeOnServer
            };
        }
    }
}
