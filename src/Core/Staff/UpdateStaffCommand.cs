namespace Core.Staff
{
    public class UpdateStaffCommand
    {
        public string EmployeeId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public bool IsBlocked { get; set; }
    }
}
