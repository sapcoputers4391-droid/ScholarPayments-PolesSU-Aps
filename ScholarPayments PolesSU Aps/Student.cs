using System;
using System.Collections.Generic;
using System.Text;

namespace ScholarPayments_PolesSU_Aps
{
    internal class Student
    {
        public int Id { get; set; }
        public string StudentCardNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string StudyForm { get; set; } = string.Empty;
        public decimal AverageGrade { get; set; }
        public Student(string studentCardNumber, string fullName, string group, string studyForm, decimal averageGrade)
        {
            StudentCardNumber = studentCardNumber;
            FullName = fullName;
            Group = group;
            StudyForm = studyForm;
            AverageGrade = averageGrade;
        }
    }
}
