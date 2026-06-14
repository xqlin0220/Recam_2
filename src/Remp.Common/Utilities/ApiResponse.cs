namespace Remp.Common.Utilities;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public ApiResponse() { }

    // 200 OK - query successful
    public static ApiResponse<T> Success(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            StatusCode = 200,
            Message = message,
            Data = data
        };
    }

    // 201 Created - New resource successfully created (POST)
    public static ApiResponse<T> Created(T data, string message = "Created successfully")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            StatusCode = 201,
            Message = message,
            Data = data
        };
    }

    // 400 Bad Request - request parameters are incorrect or verification failed
    public static ApiResponse<T> BadRequest(string message = "Bad request", List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = 400,
            Message = message,
            Errors = errors
        };
    }

    // 401 Unauthorized - Not logged in or Token is invalid
     public static ApiResponse<T> Unauthorized(string message = "Unauthorized")
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = 401,
            Message = message
        };
    }

    // 403 Forbidden - Logged in but insufficient permissions
    public static ApiResponse<T> Forbidden(string message = "Forbidden")
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = 403,
            Message = message
        };
    }

    // 404 Not Found - Resource does not exist
    public static ApiResponse<T> NotFound(string message = "Resource not found")
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = 404,
            Message = message
        };
    }

    // 500 Internal Server Error - Server internal error
    public static ApiResponse<T> ServerError(string message = "An unexpected error occurred")
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = 500,
            Message = message
        };
    }
}

// Non-generic version for operations that return no data
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse<object> Success(string message = "Success")
    {
        return new ApiResponse<object>
        {
            IsSuccess = true,
            StatusCode = 200,
            Message = message
        };
    }
}