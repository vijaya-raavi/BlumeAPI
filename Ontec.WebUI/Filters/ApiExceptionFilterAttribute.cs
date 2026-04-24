using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ontec.Core.Application.Common.Exceptions;
using System.Data.SqlClient;

namespace Ontec.WebUI.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;
        private readonly IDictionary<Type, Action<ExceptionContext>> _excepyionHandlers;
        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
        {
            _logger = logger;
            _excepyionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(ValidationException),HandleValidationException },
                { typeof(NotFoundException),HandleNotFoundException },
                { typeof(SqlException),HandleSQLException },
                { typeof(UnauthorizedAccessException),HandleUnauthorizedAccessException },
                { typeof(ForbiddenAccessException),HandleForbiddenAccessException },
            };
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception caught in API filter");
            HandleException(context);
            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            Type type = context.Exception.GetType();
            if (_excepyionHandlers.ContainsKey(type))
            {
                _excepyionHandlers[type].Invoke(context);
                return;
            }
            if (!context.ModelState.IsValid)
            {
                HandleInvalidModelStateException(context);
                return;
            }
            HandleUnknownException(context);
        }
        private void HandleSQLException(ExceptionContext context)
        {
            var exception = context.Exception;

            var details = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "SQL exception",
                Detail = exception.ToString()
            };

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }

        private void HandleInvalidModelStateException(ExceptionContext context)
        {
            var details = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            };
            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }
        private void HandleValidationException(ExceptionContext context)
        {
            var exception = (ValidationException)context.Exception;

            var details = new ValidationProblemDetails(exception.Errors)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            };
            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;

        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var exception = (NotFoundException)context.Exception;

            var details = new ProblemDetails()
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "The specified resource was not found",
                Detail = exception.Message
            };

            context.Result = new NotFoundObjectResult(details);
            context.ExceptionHandled = true;
        }
        private void HandleUnauthorizedAccessException(ExceptionContext context)
        {

            var details = new ProblemDetails()
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
            };

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            context.ExceptionHandled = true;
        }
        private void HandleForbiddenAccessException(ExceptionContext context)
        {

            var details = new ProblemDetails()
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
            };

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            context.ExceptionHandled = true;
        }
        private void HandleUnknownException(ExceptionContext context)
        {

            var details = new ProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request",
                Detail = context?.Exception?.StackTrace
            };

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.ExceptionHandled = true;
        }
    }
}
