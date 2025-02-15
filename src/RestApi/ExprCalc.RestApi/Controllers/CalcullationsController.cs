using ExprCalc.RestApi.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Controllers
{
    [ApiController]
    [Route("api/v1/calculations")]
    public class CalcullationsController(
        ILogger<CalcullationsController> logger) : ControllerBase
    {
        private readonly ILogger<CalcullationsController> _logger = logger;


        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, Description = "Calculations list")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails), Description = "Server error")]
        public Task<ActionResult<IEnumerable<CalculationDto>>> GetCalculationsListAsync(CancellationToken token)
        {
            _logger.LogDebug("Get calculations list called");
            return Task.FromResult<ActionResult<IEnumerable<CalculationDto>>>(
                new CalculationDto[] {
                    new CalculationDto { IdNum = 1 },
                    new CalculationDto { IdNum = 2 },
                    new CalculationDto { IdNum = 3 }
                });
        }
    }
}
