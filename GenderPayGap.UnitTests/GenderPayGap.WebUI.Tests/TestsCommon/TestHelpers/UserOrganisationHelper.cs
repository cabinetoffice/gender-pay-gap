using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using Moq;

namespace GenderPayGap.WebUI.Tests.TestHelpers
{
    public static class UserOrganisationHelper
    {

        //public static UserOrganisation LinkUserWithOrganisation(User user, Organisation organisation)
        //{
        //    var result = new UserOrganisation {
        //        PINConfirmedDate = VirtualDateTime.Now,
        //        User = user,
        //        Organisation = organisation
        //    };

        //    return result;
        //}

        public static UserOrganisation LinkUserWithOrganisation(User user, Organisation organisation)
        {
            var result = Mock.Of<UserOrganisation>(
                x => x.PINConfirmedDate == VirtualDateTime.Now
                     && x.User == user
                     && x.UserId == user.UserId
                     && x.Organisation == organisation
                     && x.OrganisationId == organisation.OrganisationId);

            user.UserOrganisations.Add(result);
            organisation.UserOrganisations.Add(result);

            return result;
        }

        public static object[] CreateRegistrations()
        {
            List<User> users = UserHelpers.CreateUsers();

            var organisations = new List<Organisation> {
                OrganisationHelper.GetMockedOrganisation(),
                OrganisationHelper.GetMockedOrganisation(),
                OrganisationHelper.GetMockedOrganisation()
            };

            var registrations = new List<UserOrganisation> {
                LinkUserWithOrganisation(users.Where(u => u.UserId == 23322).FirstOrDefault(), organisations[0]),
                LinkUserWithOrganisation(users.Where(u => u.UserId == 21555).FirstOrDefault(), organisations[0]),
                LinkUserWithOrganisation(users.Where(u => u.UserId == 23322).FirstOrDefault(), organisations[1]),
                LinkUserWithOrganisation(users.Where(u => u.UserId == 24572).FirstOrDefault(), organisations[2])
            };

            return new List<object> {users, organisations, registrations}.ToArray();
        }

        public static object[] CreateRegistrationsInScope()
        {
            List<User> users = UserHelpers.CreateUsers();

            var organisations = new List<Organisation> {
                OrganisationHelper.GetOrganisationInScope(),
                OrganisationHelper.GetOrganisationInScope(),
                OrganisationHelper.GetOrganisationInScope()
            };

            var registrations = new List<UserOrganisation> {
                LinkUserWithOrganisation(users.Where(u => u.UserId == 23322).FirstOrDefault(), organisations[0]),
                LinkUserWithOrganisation(users.Where(u => u.UserId == 21555).FirstOrDefault(), organisations[0]),
                LinkUserWithOrganisation(users.Where(u => u.UserId == 23322).FirstOrDefault(), organisations[1]),
                LinkUserWithOrganisation(users.Where(u => u.UserId == 24572).FirstOrDefault(), organisations[2])
            };

            return new List<object> {users, organisations, registrations}.ToArray();
        }

    }

}
