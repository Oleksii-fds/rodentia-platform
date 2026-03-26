#nullable enable
namespace Rodentia.Core.Models;

public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public static Result<T> Ok(T data) => new() { Success = true, Data = data };
    public static Result<T> Failure(string message) => new() { Success = false, ErrorMessage = message };
}

public class Result
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static Result Ok() => new() { Success = true };
    public static Result Failure(string message) => new() { Success = false, ErrorMessage = message };
}