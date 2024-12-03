using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ExternalServices;
using Microsoft.AspNetCore.Mvc;
using Notify.Models.Responses;

namespace GenderPayGap.WebUI.Services
{
    public class PinInThePostService
    {

        private const string GovUkNotifyPinInThePostLetterTemplateId = "4cc0b2e0-d8c2-43ab-a61a-9fba6575514e";
        private const int NotifyAddressLineLength = 35;

        private readonly IGovNotifyAPI govNotifyApi;

        public PinInThePostService(IGovNotifyAPI govNotifyApi)
        {
            this.govNotifyApi = govNotifyApi;
        }

        public bool GenerateAndSendPinInThePostAndUpdateUserOrganisationWithLetterId(UserOrganisation userOrganisation, IUrlHelper urlHelper)
        {
            string pin = GeneratePinInThePost();

            bool sendSuccess = SendPinInThePost(urlHelper, userOrganisation, pin, out string letterId);

            if (sendSuccess)
            {
                userOrganisation.PIN = pin;
                userOrganisation.PINSentDate = VirtualDateTime.Now;
                userOrganisation.PITPNotifyLetterId = letterId;
            }

            return sendSuccess;
        }

        public bool SendPinInThePost(IUrlHelper urlHelper, UserOrganisation userOrganisation, string pin, out string letterId)
        {
            string userFullNameAndJobTitle = $"{userOrganisation.User.Fullname} ({userOrganisation.User.JobTitle})";

            string companyName = userOrganisation.Organisation.OrganisationName;

            List<string> address = GetAddressInFourLineFormat(userOrganisation.Organisation);
            
            string postCode = userOrganisation.Organisation.GetLatestAddress().GetPostCodeInAllCaps();

            string returnUrl = urlHelper.Action("ManageOrganisationsGet", "ManageOrganisations", null, "https");
            DateTime pinExpiryDate = VirtualDateTime.Now.AddDays(Global.PinInPostExpiryDays);


            var personalisation = new Dictionary<string, dynamic>
            {
                {"address_line_1", userFullNameAndJobTitle},
                {"address_line_2", companyName},
                {"address_line_3", address.Count > 0 ? address[0] : ""},
                {"address_line_4", address.Count > 1 ? address[1] : ""},
                {"address_line_5", address.Count > 2 ? address[2] : ""},
                {"address_line_6", address.Count > 3 ? address[3] : ""},
                {"postcode", postCode},
                {"company", companyName},
                {"pin", pin},
                {"returnurl", returnUrl},
                {"expires", pinExpiryDate.ToString("d MMMM yyyy")}
            };

            LetterNotificationResponse response = govNotifyApi.SendLetter(GovUkNotifyPinInThePostLetterTemplateId, personalisation);
            if (response != null)
            {
                letterId = response.id;
                return true;
            }

            letterId = null;
            return false;
        }

        public static List<string> GetAddressInFourLineFormat(Organisation organisation)
        {
            List<string> address = GetAddressComponentsWithoutRepeatsOrUnnecessaryComponents(organisation.GetLatestAddress());

            ReduceAddressToAtMostFourLines(address);

            return address;
        }

        public static List<string> GetAddressComponentsWithoutRepeatsOrUnnecessaryComponents(OrganisationAddress organisationAddress)
        {
            var address = new List<string>();

            if (!string.IsNullOrWhiteSpace(organisationAddress.PoBox))
            {
                string poBox = organisationAddress.PoBox;
                if (!poBox.Contains("PO Box", StringComparison.OrdinalIgnoreCase))
                {
                    poBox = $"PO Box {poBox}";
                }

                address.Add("PO Box " + poBox);
            }

            if (!string.IsNullOrWhiteSpace(organisationAddress.Address1))
            {
                address.Add(organisationAddress.Address1);
            }

            if (!string.IsNullOrWhiteSpace(organisationAddress.Address2))
            {
                address.Add(organisationAddress.Address2);
            }

            if (!string.IsNullOrWhiteSpace(organisationAddress.Address3))
            {
                address.Add(organisationAddress.Address3);
            }

            if (!string.IsNullOrWhiteSpace(organisationAddress.TownCity))
            {
                address.Add(organisationAddress.TownCity);
            }

            if (!string.IsNullOrWhiteSpace(organisationAddress.County))
            {
                address.Add(organisationAddress.County);
            }

            // Gov.UK Notify can only send post to the UK, so there's no need
            // to have 'UK' or 'United Kingdom' as part of the address
            if (!string.IsNullOrWhiteSpace(organisationAddress.Country)
                && organisationAddress.Country.ToUpper() != "UNITED KINGDOM"
                && organisationAddress.Country.ToUpper() != "UK")
            {
                address.Add(organisationAddress.Country);
            }

            return address;
        }

        private static void ReduceAddressToAtMostFourLines(List<string> address)
        {
            // The address might be up to 7 lines, to start with,
            // so we might need to remove up to 3 lines
            for (var i = 0; i < 3; i++)
            {
                if (address.Count > 4)
                {
                    ReduceByOneLine(address);
                }
            }

            if (address.Count > 4)
            {
                // If the address is still more than 4 lines long, log an Error and chop off the end
                List<string> originalAddress = address.ToList(); // Take a copy of the list for the log message
                address.RemoveRange(4, address.Count - 1);

                CustomLogger.Error(
                    "PITP address is too long and has been reduced to 4 lines to fit on the Gov.UK Notify envelope",
                    new {OriginalAddress = originalAddress, ReducedAddress = address});
            }
        }

        private static void ReduceByOneLine(List<string> address)
        {
            // Loop through all pairs of lines, starting at the end of the address
            for (int currentLineNumber = address.Count - 2; currentLineNumber > 0; currentLineNumber--)
            {
                string currentLine = address[currentLineNumber];
                string nextLine = address[currentLineNumber + 1];

                if (currentLine.Length + nextLine.Length < NotifyAddressLineLength)
                {
                    string concatenatedLines = currentLine + ", " + nextLine;
                    address.RemoveRange(currentLineNumber, 2);
                    address.Insert(currentLineNumber, concatenatedLines);

                    // At this point, we've reduced the address by 1 line, so we can return
                    return;
                }
            }
        }

        public static string GeneratePinInThePost()
        {
            string pin = "";

            while (pin.Length < 7)
            {
                int randomIndex = new Random().Next(Global.PINChars.Length);
                char nextCharacter = Global.PINChars[randomIndex];
                
                pin += nextCharacter;
            }
            
            return pin;
        }

    }
}
