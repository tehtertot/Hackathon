using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hackathon.Models
{
    public class Competition : BaseEntity
    {
        public int CompetitionId { get; set; }
        
        [MinLength(2, ErrorMessage="Name must be at least 2 characters")]
        public string CompetitionName { get; set; }
        
        [PositiveRange]
        public int MaxSize { get; set; }
        
        [FutureDate]
        public DateTime Start { get; set; }
        
        [TimeLength]
        public DateTime End { get; set; }

        public int CompetitionTypeId { get; set; }
        public CompetitionType CompType { get; set; }

        public int CreatorId { get; set; }
        
        public List<Team> Teams { get; set; }

        public Team StudentTeam { get; set; }


        public Competition()
        {
            Teams = new List<Team>();
        }
        
        public override string ToString()
        {
            return $"{CompetitionName}";
        }
    }
}