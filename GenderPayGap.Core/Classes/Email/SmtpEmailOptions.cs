namespace GenderPayGap.Core.Classes
{
    public class SmtpEmailOptions
    {

        public bool? Enabled { get; set; } = true;

        public string SenderName { get; set; }

        public string SenderEmail { get; set; }

        public string ReplyEmail { get; set; }


        public string Server { get; set; }

        public string Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }


        public string Server2 { get; set; }

        public string Port2 { get; set; }

        public string Username2 { get; set; }

        public string Password2 { get; set; }

    }

}
