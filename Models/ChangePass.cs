using System.ComponentModel.DataAnnotations;

namespace Foodie.Models
{
    public class ChangePass
    {

        //[DataType(DataType.Password)]
        [Required(ErrorMessage = "Please, Enter your Current Password")]
        public string CurrentPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please, Enter your New Password")]
        public string NewPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please, Enter your Confirm Password")]
        [Compare("NewPassword",ErrorMessage ="Confirm password doesn't match")]
        public string ConfirmPassword { get; set;} = null!;
    }
}
