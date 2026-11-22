using System;
using System.Collections.Generic;
using System.Text;

namespace ScholarPayments_PolesSU_Aps
{
    public class PaymentAssignment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int PaymentTypeId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal AverageGrade { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal Coefficient { get; set; }
        public decimal Amount { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public PaymentAssignment(
            int studentId,
            int paymentTypeId,
            DateTime periodStart,
            DateTime periodEnd,
            decimal averageGrade,
            decimal baseAmount,
            decimal coefficient,
            decimal amount,
            DateTime assignmentDate,
            DateTime? paymentDate,
            string status,
            string reason)
        {
            StudentId = studentId;
            PaymentTypeId = paymentTypeId;
            PeriodStart = periodStart;
            PeriodEnd = periodEnd;
            AverageGrade = averageGrade;
            BaseAmount = baseAmount;
            Coefficient = coefficient;
            Amount = amount;
            AssignmentDate = assignmentDate;
            PaymentDate = paymentDate;
            Status = status;
            Reason = reason;
        }
    }
}
