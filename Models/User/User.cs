using System.ComponentModel.DataAnnotations;

namespace Hackathon.Models
{
    public class User : BaseEntity
    {
        [Key]
        public int UserId { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        public string Password { get; set; }
        
        public int AccessLevelId { get; set; }
        public AccessLevel Access { get; set; }

        public override string ToString() 
        {
            return $"USER: {UserId} - {FirstName} {LastName} {Email} {Password} {AccessLevelId}";
        }
    }
}