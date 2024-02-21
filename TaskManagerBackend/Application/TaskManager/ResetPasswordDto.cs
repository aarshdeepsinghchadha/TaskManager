using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.TaskManager
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
        [Required(ErrorMessage = "New Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "Password and Confirm Password do not match")]
        [DataType(DataType.Password)]
        public string NewConfirmPassword { get; set; }
    }
}
