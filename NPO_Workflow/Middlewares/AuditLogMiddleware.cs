using NPO_Workflow.DAL.Services;

namespace NPO_Workflow.Middlewares
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        public AuditLogMiddleware(RequestDelegate next)
        { 
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUser)
        {
            if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                correlationId = System.Guid.NewGuid().ToString();
            }
            string username = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "Anonymous";
            string requestPath = context.Request.Path;
            currentUser.UserName = username;
            currentUser.RequestPath = requestPath;
            currentUser.CorrelationId = correlationId.ToString();
            ///

            
            await _next(context);
        }
}
}
