using System.ComponentModel.DataAnnotations;

namespace BackOffice.Models
{
    public class ChangePasswordVm
    {
        public string TemporaryId { get; set; }

        [Required]
        [MinLength (6, ErrorMessage = "At least 6 characters")]
        [Display(Name = "New password")]
        public string Password { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "At least 6 characters")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Range(0, 9999, ErrorMessage = "Please use 4 digits as PIN")]
        [Display(Name = "New PIN")]
        public int? PIN { get; set; }

        [Range(0, 9999, ErrorMessage = "Please use 4 digits as PIN")]
        [Display(Name = "Confirm PIN")]
        public int? ConfirmPIN { get; set; }

        public string Email { get; set; }
    }

    public class ChangePin
    {
        public string TemporaryId { get; set; }

        [Required]
        [Range(0, 9999, ErrorMessage = "Please use 4 digits as PIN")]
        [Display(Name = "New PIN")]
        public int? PIN { get; set; }

        [Required]
        [Range(0, 9999, ErrorMessage = "Please use 4 digits as PIN")]
        [Display(Name = "Confirm PIN")]
        public int? ConfirmPIN { get; set; }

        public string Email { get; set; }
    }
}