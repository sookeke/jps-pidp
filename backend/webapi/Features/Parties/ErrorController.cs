namespace Pidp.Features.Parties;

using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pidp.Exceptions;
using SendGrid.Helpers.Errors.Model;


[ApiController]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    public IActionResult Error()
    {
        var exception = this.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        return this.Problem(title: exception?.Message, statusCode: GetStatusCode(exception: exception));
    }
    private static int GetStatusCode(Exception? exception) =>
 exception switch
 {
     BadRequestException => StatusCodes.Status400BadRequest,
     NotFoundException => StatusCodes.Status404NotFound,
     InValidJustinUserException => StatusCodes.Status406NotAcceptable,
     ValidationException => StatusCodes.Status422UnprocessableEntity,
     _ => StatusCodes.Status500InternalServerError
 };
}
