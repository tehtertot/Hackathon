using System;
using System.ComponentModel.DataAnnotations;

namespace Hackathon.Models
{
    public class Student : User
    {
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public int CurrStackId { get; set; }
        public MonthlyLangStack MonthlyLangStack { get; set; }

        public override string ToString() 
        {
            return $"STUDENT: {UserId} - {FirstName} {LastName} {Email} {Password} {AccessLevelId}";
        }
    }
}