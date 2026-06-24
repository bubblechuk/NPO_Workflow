using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NPO_Workflow.DAL.Models
{
    public class Operation
    {
        [Key]
        public int Id { get; set;}
        public string Name { get; set;} = string.Empty;
        public string? Instruction { get; set;}
        public float Tpz { get; set;}
        public string? PaymentType { get; set;}
        public string Section { get; set; } = string.Empty;
        public int HourLength { get; set; }
        public bool isDeleted { get; set; }

    }
}
