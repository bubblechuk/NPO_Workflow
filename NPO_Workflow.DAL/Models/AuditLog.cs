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
        public string User { get; set; }
        public string Action { get; set; }
        public string RequestPath { get; set; }
        public string Tablename { get; set; }
        public string CorrelationId { get; set; }
        public string Changes { get; set; } 

    }
}
