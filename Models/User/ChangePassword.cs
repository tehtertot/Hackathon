using System.ComponentModel.DataAnnotations;

namespace Hackathon.Models
{
    public class ChangePassword
    {
        [Required(ErrorMessage="Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name="Current Password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage="New password is required")]
        [DataType(DataType.Password)]
        [Display(Name="New Password")]
        public string NewPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name="Confirm New")]
        [Compare("NewPassword", ErrorMessage="Passwords do not match")]
        public string ConfirmNew { get; set; }
    }
}