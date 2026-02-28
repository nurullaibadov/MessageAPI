using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? Error { get; private set; }
        public List<string> Errors { get; private set; } = new();
        public int StatusCode { get; private set; }

        private Result() { }

        public static Result<T> Success(T data, int statusCode = 200)
            => new() { IsSuccess = true, Data = data, StatusCode = statusCode };

        public static Result<T> Failure(string error, int statusCode = 400)
            => new() { IsSuccess = false, Error = error, StatusCode = statusCode, Errors = new List<string> { error } };

        public static Result<T> Failure(List<string> errors, int statusCode = 400)
            => new() { IsSuccess = false, Errors = errors, Error = errors.FirstOrDefault(), StatusCode = statusCode };

        public static Result<T> NotFound(string error = "Resource not found")
            => new() { IsSuccess = false, Error = error, StatusCode = 404 };

        public static Result<T> Unauthorized(string error = "Unauthorized")
            => new() { IsSuccess = false, Error = error, StatusCode = 401 };

        public static Result<T> Forbidden(string error = "Forbidden")
            => new() { IsSuccess = false, Error = error, StatusCode = 403 };
    }

    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        public List<string> Errors { get; private set; } = new();
        public int StatusCode { get; private set; }

        public static Result Success(int statusCode = 200)
            => new() { IsSuccess = true, StatusCode = statusCode };

        public static Result Failure(string error, int statusCode = 400)
            => new() { IsSuccess = false, Error = error, StatusCode = statusCode };
    }
}
