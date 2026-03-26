#nullable enable
namespace Rodentia.Core.Models;

// 1. Базовий клас для результатів без даних (наприклад, Logout)
public class Result
{
    // Властивість називаємо IsSuccess, щоб не було конфлікту з методами
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    // Метод називаємо Ok, щоб AuthService його бачив
    public static Result Ok() => new() { IsSuccess = true };
    
    public static Result Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}

// 2. Генеричний клас для результатів з даними (наприклад, Register або GetSchedule)
public class Result<T> : Result
{
    public T? Data { get; set; }

    // Використовуємо SuccessData для чіткості
    public static Result<T> SuccessData(T data) => new() { IsSuccess = true, Data = data };

    // 'new' каже компілятору, що ми знаємо про метод у батьківському класі
    public static new Result<T> Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}