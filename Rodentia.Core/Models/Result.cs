#nullable enable
namespace Rodentia.Core.Models;


public class Result
{

    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }


    public static Result Ok() => new() { IsSuccess = true };
    
    public static Result Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };

    public static implicit operator Result(bool isSuccess) => new() { IsSuccess = isSuccess };
}


public class Result<T> : Result
{
    public T? Data { get; set; }


    public static Result<T> SuccessData(T data) => new() { IsSuccess = true, Data = data };


    public static new Result<T> Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };

    public static implicit operator Result<T>(T data) => SuccessData(data);
}