namespace api.Dtos;

public sealed record ApiResult(bool Success, string Message)
{
    public static ApiResult Ok(string message) => new(true, message);
    public static ApiResult Fail(string message) => new(false, message);
}
