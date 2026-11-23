using System;

namespace ScholarPayments_PolesSU_Aps
{
    /// <summary>
    /// Назначенная выплата. Поля соответствуют таблице dbo.PaymentAssignments.
    /// </summary>
    public class PaymentAssignment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int PaymentTypeId { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal? AverageGrade { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal Coefficient { get; set; }
        public decimal Amount { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; } = "Assigned";
        public string? Reason { get; set; }
    }
}
