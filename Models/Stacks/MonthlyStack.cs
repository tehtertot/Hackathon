using System;

namespace Hackathon.Models
{
    public class MonthlyStack : BaseEntity
    {
        public int MonthlyStackId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}