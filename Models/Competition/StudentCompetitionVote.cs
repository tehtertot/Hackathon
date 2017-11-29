namespace Hackathon.Models
{
    public class StudentCompetitionVote : BaseEntity
    {
        public int UserId { get; set; }
        public User User{ get; set; }

        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }
    }
}