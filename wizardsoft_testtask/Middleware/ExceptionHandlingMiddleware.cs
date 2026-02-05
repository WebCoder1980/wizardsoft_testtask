using System.Net;
using System.Text.Json;
using wizardsoft_testtask.Exceptions;

namespace wizardsoft_testtask.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string code;
            string message;

            if (exception is InvalidCredentialsException)
            {
                statusCode = (int)HttpStatusCode.Unauthorized;
                code = ((InvalidCredentialsException)exception).Code;
                message = exception.Message;
            }
            else if (exception is UserAlreadyExistsException)
            {
                statusCode = (int)HttpStatusCode.Conflict;
                code = ((UserAlreadyExistsException)exception).Code;
                message = exception.Message;
            }
            else if (exception is AuthException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                code = ((AuthException)exception).Code;
                message = exception.Message;
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                code = "internal_error";
                message = "Internal server error";
            }
            context.Response.StatusCode = statusCode;
            var response = new { code, message };
            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}

