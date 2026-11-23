using System;

namespace ScholarPayments_PolesSU_Aps
{
    /// <summary>
    /// Правило расчёта стипендии. Поля соответствуют таблице dbo.ScholarshipRules.
    /// </summary>
    public class ScholarshipRule
    {
        public int Id { get; set; }
        public int PaymentTypeId { get; set; }
        public string Speciality { get; set; } = string.Empty;
        public decimal MinAverageGrade { get; set; }
        public decimal MaxAverageGrade { get; set; }
        public decimal Coefficient { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
