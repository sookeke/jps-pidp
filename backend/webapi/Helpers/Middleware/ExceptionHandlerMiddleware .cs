namespace Pidp.Helpers.Middleware;

using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Pidp.Exceptions;
using SendGrid.Helpers.Errors.Model;

internal class ExceptionHandlerMiddleware
{
    /// <summary>
    /// //private readonly ILogger _logger;
    /// </summary>
    private readonly RequestDelegate _next;
    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        //_logger = logger;//
        //this.next = next;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            //this._logger.LogError(e.Message);
            await HandleExceptionAsync(context, e);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception e)
    {
        var statusCode = GetStatusCode(e);
        var response = new
        {
            title = GetTitle(e),
            status = statusCode,
            detail = e.Message,
            errors = GetErrors(e)
        };
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            BadRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            InValidJustinUserException => StatusCodes.Status406NotAcceptable,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    private static string GetTitle(Exception exception) =>
      exception switch
      {
          ApplicationException applicationException => applicationException.Message,
          InValidJustinUserException inValidJustinUserException => inValidJustinUserException.Message,
          _ => "Server Error"
      };
    private static IEnumerable<ValidationFailure> GetErrors(Exception exception)
    {
        IEnumerable<ValidationFailure> errors = null;
        if (exception is ValidationException validationException)
        {
            errors = validationException.Errors;
        }
        return errors;
    }
}
