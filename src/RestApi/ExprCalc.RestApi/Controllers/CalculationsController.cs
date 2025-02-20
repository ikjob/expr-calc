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
            try
            {
                var result = await _calculationUseCases.GetCalculationsListAsync(token);
                return Ok(result.Select(CalculationGetDto.FromEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected excpetion in {methodName}", nameof(GetCalculationsListAsync));
                throw;
            }
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected excpetion in {methodName}", nameof(CreateCalculationAsync));
                throw;
            }
        }


        [HttpPut("{id}/status")]
        [SwaggerOperation("Allow to cancel the calculation")]
        [SwaggerResponse(StatusCodes.Status200OK, Description = "Success")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails), Description = "Calculation not found or already finished")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails), Description = "Server error")]
        public async Task<ActionResult<CalculationStatusUpdateDto>> CancelCalculationAsync(Guid id, CalculationStatusPutDto status, CancellationToken token)
        {
            try
            {
                var result = await _calculationUseCases.CancelCalculationAsync(id, new Entities.User(status.CancelledBy), token);
                return Ok(CalculationStatusUpdateDto.FromEntity(result));
            }
            catch (EntityNotFoundException notFoundExc)
            {
                _logger.LogDebug(notFoundExc, "Cannot cancel calculation due to its state");

                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        type: "not_found",
                        title: "Not found",
                        detail: "Calculation not found or already finished");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected excpetion in {methodName}", nameof(CancelCalculationAsync));
                throw;
            }
        }
    }
}
