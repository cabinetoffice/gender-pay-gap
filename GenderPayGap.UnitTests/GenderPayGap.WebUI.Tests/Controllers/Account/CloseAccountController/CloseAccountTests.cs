using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Controllers.Account;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Builders;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Account.CloseAccountController
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class CloseAccountTests
    {

        [SetUp]
        public void Setup()
        {
            VirtualDateTime.Initialise(Config.OffsetCurrentDateTimeForSite());
        }

        [Test]
        [Description("POST: Closing account sets the account to retired")]
        public void POST_Closing_Account_Sets_The_Account_To_Retired()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_Password", "password");
            
            var controller = new ControllerBuilder<CloseAccountNewController>()
                .WithUserId(user.UserId)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .Build();
            
            // Act
            controller.CloseAccountPost(new CloseAccountNewViewModel()).Wait();
            
            // Assert
            Assert.AreEqual(user.Status, UserStatuses.Retired);
        }
        
        [Test]
        [Description("POST: Entering wrong password does not retire account")]
        public void POST_Entering_Wrong_Password_Does_Not_Retire_Account()
        {
            // Arrange
            User user = new UserBuilder().WithPassword("password").Build();
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_Password", "wrongpassword");
            
            var controller = new ControllerBuilder<CloseAccountNewController>()
                .WithUserId(user.UserId)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user)
                .Build();
            
            // Act
            controller.CloseAccountPost(new CloseAccountNewViewModel()).Wait();
            
            // Assert
            Assert.AreEqual(user.Status, UserStatuses.Active);
        }
        
        [Test]
        [Description("POST: Closing account removes user from organisations and emails GEO for orphans")]
        public async Task POST_Closing_Account_Removes_User_From_Organisations_And_Emails_GEO_For_Orphans()
        {
            // Arrange
            Organisation organisation1 = new OrganisationBuilder().WithOrganisationId(1).Build();
            Organisation organisation2 = new OrganisationBuilder().WithOrganisationId(2).Build();

            User standardUser = new UserBuilder()
                .WithUserId(1)
                .WithOrganisation(organisation1)
                .Build();
            
            User userToDelete = new UserBuilder()
                .WithUserId(2)
                .WithPassword("password")
                .WithOrganisation(organisation1)
                .WithOrganisation(organisation2)
                .Build();

            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Text_Password", "password");
            
            var controller = new ControllerBuilder<CloseAccountNewController>()
                .WithUserId(userToDelete.UserId)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(organisation1, organisation2, standardUser, userToDelete)
                .Build();
            
            NewUiTestHelper.MockBackgroundJobsApi
                .Setup(q => q.AddEmailToQueue(It.IsAny<NotifyEmail>()));
            
            // Act
            await controller.CloseAccountPost(new CloseAccountNewViewModel());
            
            // Assert
            // Assert that organisation1 doesn't have userToDelete associated with it, but is not an orphan
            Assert.IsEmpty(organisation1.UserOrganisations.Where(uo => uo.User.Equals(userToDelete)));
            Assert.IsFalse(organisation1.GetIsOrphan());
            
            // Assert that organisation2 is now an orphan
            Assert.IsTrue(organisation2.GetIsOrphan());
            
            // Assert that an orphan email has been sent to GEO
            NewUiTestHelper.MockBackgroundJobsApi.Verify(
                x => x.AddEmailToQueue(It.Is<NotifyEmail>(inst => inst.TemplateId == EmailTemplates.SendGeoOrphanOrganisationEmail)),
                Times.Once,
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.AccountVerificationEmail}");
        }

    }
}
