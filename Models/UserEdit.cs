using System.ComponentModel.DataAnnotations;

namespace Foodie.Models
{
    public class UserEdit
    {
        public int UserId { get; set; }

        public string Name { get; set; } = null!;

        public string Username { get; set; }= null!;

        [MinLength (10,ErrorMessage ="Atleast 10 digit required")]
        public string Mobile { get; set; }= null!;

        public string Email { get; set; }= null!;

        public string Address { get; set; }= null!;

        
        public string Passwrod { get; set; } = null!;

        public string? ImageUrl { get; set; } 

        public string UserRole { get; set; } = null!;
        public IFormFile? ProfileImg { get; set; } 
    }
}
