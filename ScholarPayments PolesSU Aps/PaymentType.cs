namespace ScholarPayments_PolesSU_Aps
{
    /// <summary>
    /// Вид выплаты. Поля соответствуют таблице dbo.PaymentTypes.
    /// </summary>
    public class PaymentType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Periodicity { get; set; } = string.Empty;
        public string FundingSource { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
