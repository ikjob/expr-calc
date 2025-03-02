using ExprCalc.CoreLogic.Api.Exceptions;
using ExprCalc.CoreLogic.Api.UseCases;
using ExprCalc.Entities.MetadataParams;
using ExprCalc.RestApi.Dto;
using ExprCalc.RestApi.Dto.Common;
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
        public async Task<ActionResult<MetadataResultDto<CalculationGetDto>>> GetCalculationsListAsync(CalculationFiltersDto filters, PaginationParamsDto pagination, CancellationToken token)
        {
            try
            {
                DateTime timeOnServer = DateTime.UtcNow;
                var result = await _calculationUseCases.GetCalculationsListAsync(filters.IntoEntity(), pagination.IntoEntity(), token);
                return Ok(
                    new MetadataResultDto<CalculationGetDto>()
                    {
                        Data = result.Items.Select(CalculationGetDto.FromEntity),
                        Metadata = QueryResultMetadataDto.FromPaginationWithTime(result, timeOnServer)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected excpetion in {methodName}", nameof(GetCalculationsListAsync));
                throw;
            }
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, Description = "Signle calculation")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails), Description = "Calculation for specified id does not exist")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails), Description = "Server error")]
        public async Task<ActionResult<DataBodyDto<CalculationGetDto>>> GetCalculationByIdAsync(Guid id, CancellationToken token)
        {
            try
            {
                var result = await _calculationUseCases.GetCalculationByIdAsync(id, token);
                return Ok(new DataBodyDto<CalculationGetDto>(CalculationGetDto.FromEntity(result)));
            }
            catch (EntityNotFoundException notFound)
            {
                _logger.LogDebug(notFound, "Entity not found. Id = {id}", id);
                return Problem(
                     statusCode: StatusCodes.Status404NotFound,
                     type: "not_found",
                     title: "Entity not found",
                     detail: "Calculation for specified key not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected excpetion in {methodName}", nameof(GetCalculationByIdAsync));
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
        [SwaggerResponse(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails), Description = "Calculation already completed")]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails), Description = "Calculation not found")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails), Description = "Server error")]
        public async Task<ActionResult<CalculationStatusUpdateDto>> CancelCalculationAsync(Guid id, CalculationStatusPutDto status, CancellationToken token)
        {
            try
            {
                var result = await _calculationUseCases.CancelCalculationAsync(id, new Entities.User(status.CancelledBy), token);
                return Ok(CalculationStatusUpdateDto.FromEntity(result));
            }
            catch (ConflictingEntityStateException confictExc)
            {
                _logger.LogDebug(confictExc, "Cannot cancel calculation due to its state");

                return Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        type: "conflict",
                        title: "Already completed",
                        detail: "Calculation already completed");
            }
            catch (EntityNotFoundException notFoundExc)
            {
                _logger.LogDebug(notFoundExc, "Cannot cancel non-existed calculation");

                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        type: "not_found",
                        title: "Not found",
                        detail: "Calculation not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected excpetion in {methodName}", nameof(CancelCalculationAsync));
                throw;
            }
        }
    }
}
