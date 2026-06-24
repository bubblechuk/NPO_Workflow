namespace NPO_Workflow.ViewModels.Operations
{
    public class OperationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Instruction { get; set; }
        public float Tpz { get; set; }
        public string? PaymentType { get; set; }
        public string Section { get; set; } = string.Empty;
        public int HourLength { get; set; }
    }
}
