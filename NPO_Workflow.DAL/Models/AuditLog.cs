using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NPO_Workflow.DAL.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string LogLevel { get; set; } = "Information";
        public string User { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string RequestPath { get; set; } = string.Empty;
        public string Tablename { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string Changes { get; set; } = string.Empty;

    }
}
