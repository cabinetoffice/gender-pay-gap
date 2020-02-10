using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Registration
{
    public partial class RegisterControllerTests
    {

        [Test]
        [Description("Ensure the Add Address form is returned correctly")]
        public void RegisterController_AddAddress_GET_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddAddress));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            var orgModel = new OrganisationViewModel {ManualRegistration = false, ManualAddress = true, SectorType = SectorTypes.Public};

            controller.StashModel(orgModel);

            //ACT:
            var result = controller.AddAddress() as ViewResult;
            var model = result?.Model as OrganisationViewModel;
            var stashedModel = controller.UnstashModel<OrganisationViewModel>();

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == nameof(RegisterController.AddAddress), "Expected Viewname=AddAddress");
            Assert.NotNull(model, "Expected model of OrganisationViewModel");
            Assert.NotNull(stashedModel, "Expected model saved to stash");
            Assert.That(model.ManualAddress, "Expected ManualAddress to be false");
        }

        [Test]
        [Description("Ensure public sector authorised with no address redirected to confirm")]
        public void RegisterController_AddAddress_POST_AuthorisedNoAddress_ToConfirm()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            var address0 = new OrganisationAddress {
                AddressId = 1,
                Status = AddressStatuses.Active,
                Source = "CoHo",
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                TownCity = "City1",
                County = "County1",
                Country = "United Kingdom",
                PoBox = "PoBox 12",
                PostCode = "W1 5qr"
            };

            var sicCode1 = new SicCode {
                SicCodeId = 2100, Description = "Desc1", SicSection = new SicSection {SicSectionId = "4321", Description = "Section1"}
            };
            var sicCode2 = new SicCode {
                SicCodeId = 10520, Description = "Desc2", SicSection = new SicSection {SicSectionId = "4326", Description = "Section2"}
            };
            var org0 = new Organisation {
                OrganisationId = 1,
                OrganisationName = "Company1",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                CompanyNumber = "12345678"
            };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName {
                OrganisationNameId = 1,
                Name = org0.OrganisationName,
                Created = org0.Created,
                Organisation = org0,
                OrganisationId = org0.OrganisationId
            };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode {
                SicCode = sicCode1,
                SicCodeId = 2100,
                Created = org0.Created,
                Organisation = org0,
                OrganisationId = org0.OrganisationId
            };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode {
                SicCode = sicCode2,
                SicCodeId = 10520,
                Created = org0.Created,
                Organisation = org0,
                OrganisationId = org0.OrganisationId
            };
            org0.OrganisationSicCodes.Add(sic2);

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddAddress));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org0, address0, name, sic1, sic2);

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord> {org0.ToEmployerRecord()};

            //use the include and exclude functions here to save typing
            var model = new OrganisationViewModel {
                NoReference = false,
                SearchText = "Searchtext",
                ManualAddress = true,
                SelectedAuthorised = true,
                ManualRegistration = false,
                SectorType = SectorTypes.Public,
                PINExpired = false,
                PINSent = false,
                SelectedEmployerIndex = 0,
                Employers = employerResult, //0 record returned
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation)
            };


            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            OrganisationViewModel savedModel = model.GetClone();
            savedModel.Address1 = "Unit 2";
            savedModel.Address2 = "Bank Road";
            savedModel.Address3 = "Essex";
            savedModel.Postcode = "PoStCode 13";
            controller.Bind(savedModel);

            //Set the expected model
            OrganisationViewModel expectedModel = savedModel.GetClone();
            expectedModel.AddressSource = user.EmailAddress;
            expectedModel.ConfirmReturnAction = nameof(RegisterController.AddAddress);

            //ACT:
            var result = controller.AddAddress(savedModel) as RedirectToActionResult;

            //ASSERTS:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == nameof(RegisterController.ConfirmOrganisation), "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unstashedModel, "Expected OrganisationViewModel");

            //Check model is same as expected
            expectedModel.Compare(unstashedModel);
        }

        [Test]
        [Description("Ensure Add Address redirects to AddContact during manual registration")]
        public void RegisterController_AddAddress_POST_ManualRegistration_RedirectToAddContact()
        {
            //ARRANGE:
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddAddress));
            routeData.Values.Add("Controller", "Register");

            var model = new OrganisationViewModel {
                OrganisationName = "Acme ltd",
                Address1 = "123",
                Address3 = "WestMinster",
                Postcode = "W1A 2ED",
                SelectedEmployerIndex = 0,
                SearchText = "smith",
                ManualRegistration = true,
                SectorType = SectorTypes.Public,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(model);
            controller.StashModel(model);

            //Set the expected model
            OrganisationViewModel expectedModel = model.GetClone();
            expectedModel.AddressSource = user.EmailAddress;

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddAddress(model) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == nameof(RegisterController.AddContact), "Redirected to the wrong action");
            Assert.NotNull(unstashedModel, "Expected Stashed OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);
        }

        [Test]
        [Description("Public Sector Manual Journey: ensure Add Contact form is returned successfully to the user")]
        public void RegisterController_AddContact_GET_PublicManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddContact));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            var model = new OrganisationViewModel {Employers = employerResult, ManualRegistration = true, SectorType = SectorTypes.Public};

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            // controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == nameof(RegisterController.AddContact), "Incorrect view returned");
            Assert.That(
                result.Model != null && result.Model.GetType() == typeof(OrganisationViewModel),
                "Expected OrganisationViewModel or Incorrect resultType returned");
        }

        [Test]
        [Description("Private Manual Journey: ensure Add Contact form is returned successfully to the user")]
        public void RegisterController_AddContact_GET_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddContact));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            var model = new OrganisationViewModel {Employers = employerResult, ManualRegistration = true, SectorType = SectorTypes.Private};

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            // controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == nameof(RegisterController.AddContact), "Incorrect view returned");
            Assert.That(
                result.Model != null && result.Model.GetType() == typeof(OrganisationViewModel),
                "Expected OrganisationViewModel or Incorrect resultType returned");
        }

        [Test]
        [Description("Ensure AddContact redirects to ConfirmOrganisation when not manual registration")]
        public void RegisterController_AddContact_POST_NotManualRegistration_RedirectToConfirmOrganisation()
        {
            //ARRANGE:
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddContact));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);

            var model = new OrganisationViewModel {
                OrganisationName = "Acme ltd",
                Address1 = "123",
                Address3 = "WestMinster",
                Postcode = "W1A 2ED",
                SelectedEmployerIndex = 0,
                SearchText = "smith",
                ManualRegistration = false,
                ManualAddress = true,
                SectorType = SectorTypes.Public,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(model);

            OrganisationViewModel savedModel = model.GetClone();
            savedModel.ContactEmailAddress = "test@hotmail.com2";
            savedModel.ContactFirstName = "test firstName2";
            savedModel.ContactLastName = "test lastName2";
            savedModel.ContactJobTitle = "test job title2";
            savedModel.ContactPhoneNumber = "79004 123 456";
            controller.Bind(savedModel);

            //Set the expected model
            OrganisationViewModel expectedModel = model.GetClone();
            expectedModel.ConfirmReturnAction = nameof(RegisterController.AddContact);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact(model) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult", "Wrong redirect");
            Assert.That(result.ActionName == nameof(RegisterController.ConfirmOrganisation), "Redirected to the wrong action");
            Assert.NotNull(unstashedModel, "Expected Stashed OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);
        }

        //[Ignore("This test needs fixing")]
        [Test]
        [Description("Private Manual Journey: ensure Add Contact form is filled and sent successfully")]
        public void RegisterController_AddContact_POST_PrivateSector_ManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddContact));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            var expectedModel = new OrganisationViewModel {
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                CompanyNumber = "12345678",
                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactPhoneNumber = "79000 000 000",
                Country = "United Kingdom",
                OrganisationName = "Acme ltd",
                PINExpired = false,
                PINSent = false,
                PoBox = "",
                Postcode = "W1 5qr",
                ReviewCode = "",
                SearchText = "Searchtext",
                //   SelectedEmployerIndex = -1,
                BackAction = "",
                CancellationReason = "",
                Employers = employerResult,
                ManualRegistration = true,
                SectorType = SectorTypes.Private
            };


            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            controller.Bind(expectedModel);

            //Stash the object for the unstash to happen in code
            controller.StashModel(expectedModel);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact(expectedModel) as RedirectToActionResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == nameof(RegisterController.AddSector), "Redirected to the wrong view");
            var actualModel = controller.UnstashModel<OrganisationViewModel>();
            Assert.NotNull(expectedModel, "Expected OrganisationViewModel");
            actualModel.Compare(expectedModel);
        }

        [Test]
        [Description("Public Manual:ensure Add Contact form is returned successfully to the user")]
        public void RegisterController_AddContact_POST_PublicSector_ManualRegistration_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddContact));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();


            var expectedModel = new OrganisationViewModel {
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                CompanyNumber = "12345678",
                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactPhoneNumber = "79000 000 000",
                Country = "United Kingdom",
                OrganisationName = "Acme ltd",
                PINExpired = false,
                PINSent = false,
                PoBox = "",
                Postcode = "W1 5qr",
                ReviewCode = "",
                SearchText = "Searchtext",
                //   SelectedEmployerIndex = -1,
                BackAction = "",
                CancellationReason = "",
                Employers = employerResult,
                ManualRegistration = true,
                SectorType = SectorTypes.Public
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(expectedModel);

            //Stash the object for the unstash to happen in code
            controller.StashModel(expectedModel);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact(expectedModel) as RedirectToActionResult;

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == nameof(RegisterController.AddSector), "Redirected to the wrong view");
            var actualModel = controller.UnstashModel<OrganisationViewModel>();
            Assert.NotNull(expectedModel, "Expected OrganisationViewModel");
            actualModel.Compare(expectedModel);
        }

        [Test]
        [Description("Ensure that the AddContact form values are saved correctly")]
        public void RegisterController_AddContact_POST_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddContact));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            //use the include and exclude functions here to save typing
            var model = new OrganisationViewModel {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                ManualAddress = false,
                AddressSource = "CoHo",
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                City = "City1",
                County = "County1",
                Country = "United Kingdom",
                PoBox = "PoBox 12",
                Postcode = "W1 5qr",
                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactPhoneNumber = "79000 000 000",
                SicCodeIds = "1,2,3,4",
                SicSource = "CoHo",
                SearchText = "Searchtext",
                ManualRegistration = false,
                SicCodes = new List<int>(),
                SectorType = SectorTypes.Private,
                PINExpired = false,
                PINSent = false,
                SelectedEmployerIndex = 0,
                Employers = employerResult, //0 record returned
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation)
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            OrganisationViewModel savedModel = model.GetClone();
            savedModel.ContactEmailAddress = "test@hotmail.com2";
            savedModel.ContactFirstName = "test firstName2";
            savedModel.ContactLastName = "test lastName2";
            savedModel.ContactJobTitle = "test job title2";
            savedModel.ContactPhoneNumber = "79004 123 456";
            controller.Bind(savedModel);
            OrganisationViewModel expectedModel = savedModel.GetClone();

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddContact(savedModel) as RedirectToActionResult;

            //ASSERTS:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == nameof(RegisterController.AddSector), "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unstashedModel, "Expected OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);
        }

        [Test]
        [Description("Ensure Add Sector form is returned successfully to the user")]
        public void RegisterController_AddSector_GET_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddSector));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            var model = new OrganisationViewModel {Employers = employerResult, ManualRegistration = true, SectorType = SectorTypes.Private};

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            // controller.Bind(model);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddSector() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == nameof(RegisterController.AddSector), "Incorrect view returned");
            Assert.That(
                result.Model != null && result.Model.GetType() == typeof(OrganisationViewModel),
                "Expected OrganisationViewModel or Incorrect resultType returned");
        }

        [Test]
        [Description("Ensure that the AddSector form values are saved correctly")]
        public void RegisterController_AddSector_POST_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddSector));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            //use the include and exclude functions here to save typing
            var model = new OrganisationViewModel {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                ManualAddress = false,
                AddressSource = "CoHo",
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                City = "City1",
                County = "County1",
                Country = "United Kingdom",
                PoBox = "PoBox 12",
                Postcode = "W1 5qr",
                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactPhoneNumber = "79000 000 000",
                SicCodeIds = "1,2,3,4",
                SicSource = "CoHo",
                SearchText = "Searchtext",
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                PINExpired = false,
                PINSent = false,
                SelectedEmployerIndex = 0,
                Employers = employerResult, //0 record returned
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation)
            };

            var sic1 = new SicCode {
                SicCodeId = 1420, SicSectionId = "SSID1", SicSection = new SicSection {SicSectionId = "5221"}, Description = ""
            };
            var sic2 = new SicCode {
                SicCodeId = 14310, SicSectionId = "SSID1", SicSection = new SicSection {SicSectionId = "4115"}, Description = ""
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user, sic1, sic2);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            OrganisationViewModel savedModel = model.GetClone();
            savedModel.SicCodeIds = "1420,14310";
            controller.Bind(savedModel);
            OrganisationViewModel expectedModel = savedModel.GetClone();
            expectedModel.SicSource = user.EmailAddress;
            expectedModel.ConfirmReturnAction = nameof(RegisterController.AddSector);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.AddSector(savedModel) as RedirectToActionResult;

            //ASSERTS:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == nameof(RegisterController.ConfirmOrganisation), "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unstashedModel, "Expected OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);
        }

    }
}
