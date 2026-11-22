using System;
using System.Collections.Generic;
using System.Text;

namespace ScholarPayments_PolesSU_Aps
{
    public class ScholarshipCalculator
    {
        public decimal CalculateAmount(
            decimal averageGrade,
            string educationProfile,
            decimal baseAmount,
            List<ScholarshipRule> rules,
            DateTime calculationDate)
        {
            if (averageGrade < 0 || averageGrade > 10)
                throw new ArgumentException("Некорректный средний балл.");

            if (baseAmount <= 0)
                throw new ArgumentException("Базовая сумма должна быть больше нуля.");
            ScholarshipRule rule = rules.FirstOrDefault(r =>
                r.IsActive &&
                string.Equals(
                    r.EducationProfile,
                    educationProfile,
                    StringComparison.OrdinalIgnoreCase) &&
                averageGrade >= r.MinAverageGrade &&
                averageGrade <= r.MaxAverageGrade &&
                r.ValidFrom <= calculationDate &&
                (r.ValidTo == null || r.ValidTo >= calculationDate)
            );
            if (rule == null)
                throw new InvalidOperationException(
                    "Подходящее правило расчёта не найдено.");
            decimal result = baseAmount * rule.Coefficient;

            return result;
        }
    }
}
