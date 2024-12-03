namespace GenderPayGap.WebUI.Helpers
{
    public static class EmailAddressHelper
    {

        public static bool DomainNameMatches(string emailOne, string emailTwo)
        {
            try
            {
                string domainOne = emailOne.Substring(emailOne.LastIndexOf('@') + 1);
                string domainTwo = emailTwo.Substring(emailTwo.LastIndexOf('@') + 1);
                return string.Equals(domainOne, domainTwo);
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
