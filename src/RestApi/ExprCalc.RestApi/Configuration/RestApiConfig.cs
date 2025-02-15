using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Configuration
{
    public class RestApiConfig
    {
        public const string ConfigurationSectionName = "RestAPI";

        public bool CorsAllowAny { get; init; } = false;
        public bool UseSwagger { get; init; } = false;
    }
}
