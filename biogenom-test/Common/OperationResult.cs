namespace biogenom_test.Common;

public class OperationResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string ErrorMessage { get; }

    private OperationResult(bool isSuccess, T value, string errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static OperationResult<T> Success(T value) => new OperationResult<T>(true, value, null);
    public static OperationResult<T> Failure(string errorMessage) => new OperationResult<T>(false, default, errorMessage);
}

public class Unit
{
    public static readonly Unit Value = new Unit();
    private Unit() { }
}

