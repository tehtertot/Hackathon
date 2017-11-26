namespace Hackathon.Models
{
    public class MonthlyLangStack : BaseEntity
    {
        public int MonthlyLangStackId { get; set; }
        
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        public int MonthtlyStackId { get; set; }
        public MonthlyStack Stack { get; set; }

        public int InstructorId { get; set; }
        public User Instructor { get; set; }

        public override string ToString() 
        {
            return $"{Language.LanguageName} - starting {Stack.StartDate.ToString("d")} ({Instructor.FirstName})";
        }
    }
}