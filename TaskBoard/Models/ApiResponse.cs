namespace TaskBoard.Models;

public enum ApiResponseCode
{
    Ok = 0,
    Error = 1,
    MaximumAccounts = 2,
    ArgumentsValidationFailed = 3,
    NoPhoneVerificationApiSet = 4,
    Unauthorized = 5
}

public class ApiResponse
{
    public string? Message { get; set; }

    // Data returning from the api. This could be a string or a more complex object
    public object? Data { get; set; }
    public ApiResponseCode Code { get; set; }
}