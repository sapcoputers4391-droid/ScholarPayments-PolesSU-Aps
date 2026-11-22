using System;
using System.Collections.Generic;
using System.Text;

namespace ScholarPayments_PolesSU_Aps
{
    public class PaymentType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Periodicity { get; set; }
        public string CalculationMethod { get; set; }
        public bool IsActive { get; set; }
        public PaymentType(
            string name,
            string category,
            string description,
            string periodicity,
            string calculationMethod,
            bool isActive = true)
        {
            Name = name;
            Category = category;
            Description = description;
            Periodicity = periodicity;
            CalculationMethod = calculationMethod;
            IsActive = isActive;
        }
    }
}
