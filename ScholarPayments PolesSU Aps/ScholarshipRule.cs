using System;
using System.Collections.Generic;
using System.Text;

namespace ScholarPayments_PolesSU_Aps
{
    public class ScholarshipRule
    {
        public int Id { get; set; }
        public int PaymentTypeId { get; set; }
        public string EducationProfile { get; set; }
        public decimal MinAverageGrade { get; set; }
        public decimal MaxAverageGrade { get; set; }
        public decimal Coefficient { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public ScholarshipRule(
            int paymentTypeId,
            string educationProfile,
            decimal minAverageGrade,
            decimal maxAverageGrade,
            decimal coefficient,
            DateTime validFrom,
            DateTime? validTo,
            bool isActive = true)
        {
            PaymentTypeId = paymentTypeId;
            EducationProfile = educationProfile;
            MinAverageGrade = minAverageGrade;
            MaxAverageGrade = maxAverageGrade;
            Coefficient = coefficient;
            ValidFrom = validFrom;
            ValidTo = validTo;
            IsActive = isActive;
        }
    }
}
