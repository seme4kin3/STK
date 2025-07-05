using Microsoft.AspNetCore.Http;

namespace STK.Application.Middleware
{
    public class DomainException : Exception
    {
        public int StatusCode { get; }

        public DomainException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
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
            new DomainException(message, StatusCodes.Status204NoContent);
    }
}
