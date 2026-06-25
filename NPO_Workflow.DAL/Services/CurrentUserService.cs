namespace NPO_Workflow.DAL.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public string UserName { get; set; } = "Anonymous";
        public string? CorrelationId { get; set; }
        public string? RequestPath { get; set; }
    }
}
