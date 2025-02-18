using ExprCalc.CoreLogic.Api.Exceptions;
using ExprCalc.CoreLogic.Api.UseCases;
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
    public class CalculationsController(
        ICalculationUseCases calculationUseCases,
        ILogger<CalculationsController> logger) : ControllerBase
    {
        private readonly ICalculationUseCases _calculationUseCases = calculationUseCases;
        private readonly ILogger<CalculationsController> _logger = logger;


        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, Description = "Calculations list")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails), Description = "Server error")]
        public async Task<ActionResult<IEnumerable<CalculationGetDto>>> GetCalculationsListAsync(CancellationToken token)
        {
            var result = await _calculationUseCases.GetCalculationsListAsync(token);
            return Ok(result.Select(CalculationGetDto.FromEntity));
        }


        [HttpPost]
        [SwaggerResponse(StatusCodes.Status200OK, Description = "Success")]
        [SwaggerResponse(StatusCodes.Status429TooManyRequests, Type = typeof(ProblemDetails), Description = "Too many pedning calculations")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails), Description = "Server error")]
        public async Task<ActionResult<CalculationGetDto>> CreateCalculationAsync(CalculationCreateDto calculation, CancellationToken token)
        {
            try
            {
                var result = await _calculationUseCases.CreateCalculationAsync(calculation.IntoEntity(), token);
                return Ok(CalculationGetDto.FromEntity(result));
            }
            catch (TooManyPendingCalculationsException tooManyCalcsExc)
            {
                _logger.LogDebug(tooManyCalcsExc, "Too many pedning calculations. New one is rejected");

                return Problem(
                        statusCode: StatusCodes.Status429TooManyRequests,
                        type: "overflow",
                        title: "Server overloaded",
                        detail: "Too many pending calculations");
            }
        }
    }
}
