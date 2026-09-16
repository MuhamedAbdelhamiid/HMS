using Microsoft.AspNetCore.Http;

namespace HMS.Shared.Responses
{
    public class GenericResponse<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }

        public static GenericResponse<T> Success(T data, string message, int statusCode = StatusCodes.Status200OK)
        {
            return new GenericResponse<T>
            {
                Data = data,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static GenericResponse<T> Error(string message, int statusCode = StatusCodes.Status400BadRequest)
        {
            return new GenericResponse<T>
            {
                Data = default,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static GenericResponse<T> Failure(string message)
        {
            return new GenericResponse<T>
            {
                Data = default,
                Message = message,
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}