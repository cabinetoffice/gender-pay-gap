namespace GenderPayGap.Core.Abstractions
{

    public abstract class AEmailTemplate
    {

        /// <summary>
        ///     The email address that will receive the email
        /// </summary>
        public string RecipientEmailAddress { get; set; }

        /// <summary>
        ///     Determine if the email provider should be sent using a test api key
        /// </summary>
        public bool Test { get; set; }

        /// <summary>
        ///     Will simulate sending an email
        ///     The email provider will return a successful result without sending an email.
        ///     Refactor: Integration tests should implement a test email provider rather than use a flag like this
        /// </summary>
        public bool Simulate { get; set; }

    }

}
