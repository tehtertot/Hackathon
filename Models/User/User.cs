using System.ComponentModel.DataAnnotations;

namespace Hackathon.Models
{
    public class User : BaseEntity
    {
        [Key]
        public int UserId { get; set; }

        [Display(Name="First Name")]
        public string FirstName { get; set; }
        
        [Display(Name="Last Name")]
        public string LastName { get; set; }
        
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool ChangePassword { get; set; }
        
        public int AccessLevelId { get; set; }
        public AccessLevel Access { get; set; }


        public override string ToString() 
        {
            return $"USER: {UserId} - {FirstName} {LastName} {Email} {Password} {AccessLevelId}";
        }
    }
}