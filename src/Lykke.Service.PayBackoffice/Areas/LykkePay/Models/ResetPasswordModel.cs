namespace BackOffice.Areas.LykkePay.Models
{
    public class ResetPasswordModel
    {
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string EmailTemplate { get; set; }
    }
}
