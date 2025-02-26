using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Dto.Common
{
    public record struct DataBodyDto<T>(T Data);
}
