namespace Core.Messages.Sms
{
    public class SmsMessage
    {
        public string From { get; set; }
        public string Text { get; set; }

        public static SmsMessage Create(string from, string text)
        {
            return new SmsMessage
            {
                From = from,
                Text = text
            };
        }
    }
}
