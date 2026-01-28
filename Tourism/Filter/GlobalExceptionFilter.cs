
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;

namespace Tourism.Filter;

public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {

        var statusCode = 500;
        var message = "Internal Server Error";
        switch (context.Exception)
        {
            case DbUpdateException e:
                {
                    statusCode = 500;
                    message = "Database error";
                    break;
                }
            case ArgumentException:
                {
                    statusCode = 400;
                    message = context.Exception.Message;
                    break;
                }
            case UnauthorizedAccessException:
                {
                    statusCode = 401;
                    message = "You are not authorized to access this resource.";
                    break;
                }
            default:
                {
                    logger.LogError(context.Exception, "An unhandled exception occurred.");
                    break;
                }
        }
        var response = new
        {
            statusCode,
            timestamp = DateTime.UtcNow,
            path = context.HttpContext.Request.Path,
            message,
            details = context.Exception.StackTrace,
        };
        context.Result = new ObjectResult(response) { StatusCode = statusCode };
        context.ExceptionHandled = true;
    }
}
