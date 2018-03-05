namespace LkeServices.Messages.EmailTemplates.ViewModels
{
    public class RemindPasswordTemplate
    {
        public RemindPasswordTemplate(string hint, int year)
        {
            Hint = hint;
            Year = year;
        }

        public string Hint { get; set; }
        public int Year { get; set; }
    }
}
