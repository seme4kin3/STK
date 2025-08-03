using Microsoft.AspNetCore.Http;

namespace STK.Application.Middleware
{
    public class DomainException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public Dictionary<string, object> Details { get; }

        public DomainException(string message, int statusCode, string errorCode = null, Dictionary<string, object> details = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode ?? "DOMAIN_ERROR";
            Details = details ?? new Dictionary<string, object>();
        }

        public static DomainException Conflict(string message) =>
            new DomainException(message, StatusCodes.Status409Conflict);

        public static DomainException ConflictSubscription(string message) =>
            new DomainException(message, StatusCodes.Status409Conflict);

        public static DomainException Forbidden(string message) =>
            new DomainException(message, StatusCodes.Status403Forbidden);

        public static DomainException Unauthorized(string message) =>
            new DomainException(message, StatusCodes.Status401Unauthorized);

        public static DomainException UserNotFound(string message) =>
            new DomainException(message, StatusCodes.Status404NotFound);
        public static DomainException TooManyAttempts(string message) =>
            new DomainException(message, StatusCodes.Status429TooManyRequests, "TOO_MANY_ATTEMPTS");
    }
}
