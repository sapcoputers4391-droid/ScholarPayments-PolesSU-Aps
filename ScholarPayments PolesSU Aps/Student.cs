namespace ScholarPayments_PolesSU_Aps
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentCardNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string Speciality { get; set; } = string.Empty;
        public string StudyGroup { get; set; } = string.Empty;
        public int Course { get; set; } = 1;
        public string StudyForm { get; set; } = string.Empty;
        public string FundingType { get; set; } = string.Empty;
        public decimal AverageGrade { get; set; }
    }
}
