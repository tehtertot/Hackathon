namespace Hackathon.Models
{
    public class MonthlyLangStackCompetition : BaseEntity
    {
        public int MonthlyLangStackId { get; set; }
        public MonthlyLangStack MonthlyLangStack { get; set; }

        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }
    }
}