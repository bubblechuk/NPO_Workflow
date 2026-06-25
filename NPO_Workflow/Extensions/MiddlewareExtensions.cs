using NPO_Workflow.Middlewares;

namespace NPO_Workflow.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditLog(this IApplicationBuilder app) 
        { 
            return app.UseMiddleware<AuditLogMiddleware>();
        }
    }
}
