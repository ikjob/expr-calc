using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.RestApi.Helpers
{
    /// <summary>
    /// Converts unhandled exceptions into ProblemDetails
    /// </summary>
    internal class ExceptionToProblemDetailsHandler : Microsoft.AspNetCore.Diagnostics.IExceptionHandler
    {
        private readonly bool _isDevelopmentEnv;
        public ExceptionToProblemDetailsHandler(IHostEnvironment environment)
        {
            _isDevelopmentEnv = environment.IsDevelopment();
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Type = "internal",
                Title = "Internal server error",
                Detail = _isDevelopmentEnv ? $"{exception.GetType().Name}: {exception.Message}" : null,
                Status = StatusCodes.Status500InternalServerError
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
