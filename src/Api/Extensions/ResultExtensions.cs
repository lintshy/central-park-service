using CentralPark.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CentralPark.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result) =>
        result.IsSuccess
            ? new OkObjectResult(result.Value)
            : MapError(result.Error);

    public static IActionResult ToActionResult(this Result result) =>
        result.IsSuccess
            ? new NoContentResult()
            : MapError(result.Error);

    public static IActionResult ToCreatedResult<T>(this Result<T> result, string routeName, object routeValues) =>
        result.IsSuccess
            ? new CreatedAtRouteResult(routeName, routeValues, result.Value)
            : MapError(result.Error);

    public static IActionResult ToAcceptedResult(this Result result) =>
        result.IsSuccess
            ? new AcceptedResult()
            : MapError(result.Error);

    private static IActionResult MapError(Error error) =>
        error.Code.Contains("NotFound")
            ? new NotFoundObjectResult(error)
            : error.Code.Contains("Forbidden")
                ? new ObjectResult(error) { StatusCode = StatusCodes.Status403Forbidden }
                : error.Code.Contains("Unauthorized") || error.Code.Contains("InvalidCredentials")
                    ? new UnauthorizedObjectResult(error)
                    : error.Code.Contains("AlreadyExists") || error.Code.Contains("InvalidTransition") || error.Code.Contains("NotAcceptingOrders")
                        ? new ConflictObjectResult(error)
                        : new BadRequestObjectResult(error);
}
