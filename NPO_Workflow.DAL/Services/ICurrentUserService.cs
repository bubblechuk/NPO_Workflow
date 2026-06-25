namespace NPO_Workflow.DAL.Services
{
    public interface ICurrentUserService
{
    string UserName { get; set; }
    string? CorrelationId { get; set; }
    string? RequestPath { get; set; }
}
}
