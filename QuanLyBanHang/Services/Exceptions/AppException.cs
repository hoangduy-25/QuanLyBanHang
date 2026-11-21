using System;
using System.Net;

namespace QuanLyBanHang.Services.Exceptions
{
    /// <summary>
    /// Ngoại lệ nghiệp vụ có gắn HTTP Status Code để Global Exception Handler ánh xạ thống nhất.
    /// </summary>
    public class AppException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public object Details { get; private set; }

        public AppException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object details = null)
            : base(message)
        {
            StatusCode = statusCode;
            Details = details;
        }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message, object details = null)
            : base(message, HttpStatusCode.BadRequest, details) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message)
            : base(message, HttpStatusCode.Unauthorized) { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message)
            : base(message, HttpStatusCode.Forbidden) { }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base(message, HttpStatusCode.NotFound) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message, object details = null)
            : base(message, HttpStatusCode.Conflict, details) { }
    }
}
