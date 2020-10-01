using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Controllers.Account;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Tests.Builders;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Account.CloseAccountController
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class CloseAccountTests
    {

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
        public void POST_Closing_Account_Removes_User_From_Organisations_And_Emails_GEO_For_Orphans()
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
            
            // Act
            controller.CloseAccountPost(new CloseAccountNewViewModel()).Wait();
            
            // Assert
            Assert.IsEmpty(organisation1.UserOrganisations.Where(uo => uo.User.Equals(userToDelete)));
            Assert.IsFalse(organisation1.GetIsOrphan());
            
            Assert.IsTrue(organisation2.GetIsOrphan());
        }

    }
}
