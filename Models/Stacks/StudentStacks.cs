namespace Hackathon.Models
{
    public class StudentStack : BaseEntity
    {
        public int MonthlyLangStackId { get; set; }
        public MonthlyLangStack MonthlyLangStack { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}