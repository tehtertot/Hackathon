using System.Collections.Generic;

namespace Hackathon.Models
{
    public class Team : BaseEntity
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string ProjectTitle { get; set; }

        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }

        public List<Student> Students { get; set; }

        public Team() {
            Students = new List<Student>();
        }

        public override string ToString()
        {
            return $"{TeamName} - {ProjectTitle}";
        }
    }
}