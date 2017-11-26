using System;

namespace Hackathon.Models
{
    public class Student : User
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int CurrStackId { get; set; }
        public MonthlyLangStack MonthlyLangStack { get; set; }

        public User UserInfo { get; set; }

        public override string ToString() 
        {
            return $"STUDENT: {UserId} - {FirstName} {LastName} {Email} {Password} {AccessLevelId}";
        }
    }
}