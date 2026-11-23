using System;
using System.Collections.Generic;
using System.Linq;

namespace ScholarPayments_PolesSU_Aps
{
    /// <summary>
    /// Расчёт размера стипендии по правилам таблицы dbo.ScholarshipRules.
    /// Сумма = базовая сумма × коэффициент × количество месяцев.
    /// </summary>
    public class ScholarshipCalculator
    {
        public decimal CalculateAmount(
            decimal averageGrade,
            string? speciality,
            decimal baseAmount,
            List<ScholarshipRule> rules,
            DateTime calculationDate,
            int months = 1)
        {
            if (averageGrade < 0 || averageGrade > 10)
                throw new ArgumentException("Некорректный средний балл.");

            if (baseAmount <= 0)
                throw new ArgumentException("Базовая сумма должна быть больше нуля.");

            if (months < 1)
                throw new ArgumentException("Количество месяцев должно быть не меньше одного.");

            ScholarshipRule? rule = FindRule(averageGrade, speciality, rules, calculationDate);
            if (rule == null)
                throw new InvalidOperationException(
                    "Подходящее правило расчёта не найдено.");

            return Math.Round(baseAmount * rule.Coefficient * months, 2);
        }

        /// <summary>
        /// Ищет подходящее правило: сначала по специальности студента,
        /// затем правило по умолчанию (пустая специальность).
        /// </summary>
        public ScholarshipRule? FindRule(
            decimal averageGrade,
            string? speciality,
            List<ScholarshipRule> rules,
            DateTime calculationDate)
        {
            return rules
                .Where(r => r.IsActive)
                .Where(r => averageGrade >= r.MinAverageGrade && averageGrade <= r.MaxAverageGrade)
                .Where(r => r.ValidFrom <= calculationDate)
                .Where(r => r.ValidTo == null || r.ValidTo >= calculationDate)
                .Where(r => string.IsNullOrWhiteSpace(r.Speciality)
                            || string.Equals(r.Speciality, speciality, StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => string.IsNullOrWhiteSpace(r.Speciality) ? 1 : 0)
                .FirstOrDefault();
        }

        /// <summary>
        /// Количество месяцев в периоде включительно (сентябрь–январь = 5).
        /// </summary>
        public static int CountMonths(DateTime periodStart, DateTime periodEnd)
        {
            if (periodEnd < periodStart)
                throw new ArgumentException("Дата окончания периода раньше даты начала.");

            return (periodEnd.Year - periodStart.Year) * 12
                   + periodEnd.Month - periodStart.Month + 1;
        }
    }
}
