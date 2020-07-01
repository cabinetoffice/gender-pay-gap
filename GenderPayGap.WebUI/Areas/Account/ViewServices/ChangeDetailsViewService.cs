using System;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Areas.Account.Abstractions;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.BusinessLogic.Models.Account;

namespace GenderPayGap.WebUI.Areas.Account.ViewServices
{

    public class ChangeDetailsViewService : IChangeDetailsViewService
    {

        public ChangeDetailsViewService(IUserRepository userRepo)
        {
            UserRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        private IUserRepository UserRepository { get; }

        public bool ChangeDetails(ChangeDetailsViewModel newDetails, User currentUser)
        {
            // map to business domain model
            var mappedDetails = new UpdateDetailsModel
            {
                FirstName = newDetails.FirstName,
                LastName = newDetails.LastName,
                JobTitle = newDetails.JobTitle,
                ContactPhoneNumber = newDetails.ContactPhoneNumber,
                SendUpdates = newDetails.SendUpdates,
                AllowContact = newDetails.AllowContact
            };

            // execute update details
            return UserRepository.UpdateDetails(currentUser, mappedDetails);
        }

    }

}
