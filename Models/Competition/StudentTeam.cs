namespace Hackathon.Models
{
    public class StudentTeam : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }
    }
}