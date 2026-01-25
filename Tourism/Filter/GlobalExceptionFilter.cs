using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tourism.Filter;

public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        logger.LogError(context.Exception, "An unhandled exception occurred.");

        var statusCode = 500;
        var message = "Internal Server Error";

        if (context.Exception is ArgumentException)
        {
            statusCode = 400;
            message = context.Exception.Message;
        }
        else if (context.Exception is UnauthorizedAccessException)
        {
            statusCode = 401;
            message = "You are not authorized to access this resource.";
        }

        var response = new
        {
            statusCode,
            timestamp = DateTime.UtcNow,
            path = context.HttpContext.Request.Path,
            message,

            details = context.Exception.StackTrace
        };

        context.Result = new ObjectResult(response)
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}