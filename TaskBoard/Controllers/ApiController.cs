using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;
using TaskBoard.Models.Datatables;

namespace TaskBoard.Controllers;

public class ApiController : ControllerBase
{
    public OkObjectResult OkApi(string? message = "", object? data = null)
    {
        return Ok(new ApiResponse {Message = message, Data = data, Code = ApiResponseCode.Ok});
    }

    public UnauthorizedObjectResult UnauthorizedApi(string? message = "", object? data = null, ApiResponseCode code = ApiResponseCode.Error)
    {
        return Unauthorized(new ApiResponse {Message = message, Data = data, Code = code});
    }

    public BadRequestObjectResult BadRequestApi(string? message = "", object? data = null, ApiResponseCode code = ApiResponseCode.Error)
    {
        return BadRequest(new ApiResponse {Message = message, Data = data, Code = code});
    }
}