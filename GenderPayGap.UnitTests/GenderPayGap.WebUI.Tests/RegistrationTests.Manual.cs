using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Extensions;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Repositories;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using GenderPayGap.WebUI.Models;
using System.Threading.Tasks;

namespace GenderPayGap.WebUI.Tests.Controllers.Registration
{
    [TestFixture]
    public partial class RegisterControllerTests
    {
        #region Organisation Type

        [Test]
        [Ignore("Needs fixing/deleting")]
        [Description("RegisterController.OrganisationType GET: When PendingFastrack Then start Fasttrack registration")]
        public void RegisterController_OrganisationType_GET_When_PendingFastrack_Then_StartFastTrackRegistration()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Description("Ensure the Organisation type form is returned for the current user ")]
        public void RegisterController_OrganisationType_GET_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            //ACT:
            var result = controller.OrganisationType() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "OrganisationType", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Ignore("Needs fixing/deleting")]
        [Description("Check registration completes successfully during fasttrack registration")]
        public void RegisterController_OrganisationType_GET_FastTrackRegistration_ServiceActivated()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var org = new Organisation() { OrganisationId = 1, SectorType = SectorTypes.Private };
            var address = new OrganisationAddress() { AddressId = 1, OrganisationId = 1, Organisation = org, Status = AddressStatuses.Pending };
            var orgScope = new OrganisationScope() { OrganisationId = org.OrganisationId, RegisterStatus = RegisterStatuses.RegisterPending, ContactEmailAddress = user.EmailAddress };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.OrganisationType));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org, address, orgScope);

            var mockScopeBL = AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);
            //ACT:
            var result = controller.OrganisationType() as RedirectToActionResult;
            var userOrg = controller.DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == user.UserId && uo.OrganisationId == orgScope.OrganisationId);

            //ASSERT:
            Assert.That(result != null, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "ServiceActivated", "Expected redirect to ServiceActivated");
            Assert.That(userOrg.PINConfirmedDate > DateTime.MinValue);
            Assert.That(userOrg.Organisation.Status == OrganisationStatuses.Active);
            Assert.That(userOrg.Organisation.GetAddress().AddressId == address.AddressId);
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId);
            Assert.That(address.Status == AddressStatuses.Active);
            Assert.That(orgScope.RegisterStatus == RegisterStatuses.RegisterComplete);
            Assert.That(orgScope.RegisterStatusDate > DateTime.MinValue);
        }

        [Test]
        [Description("Private Sector:Ensure the Organisation type form is confirmed and sent successfully")]
        public void RegisterController_OrganisationType_POST_PrivateSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, Organisation = organisation, UserId = 1, PINConfirmedDate = VirtualDateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var expectedModel = new OrganisationViewModel() {
                ManualRegistration = false,
                SectorType = SectorTypes.Private
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user/*, userOrganisation, organisation*/);
            controller.Bind(expectedModel);

            //Stash the object for the unstash to happen in code
            controller.StashModel(expectedModel);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.OrganisationType(expectedModel) as RedirectToActionResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName.ToString() == "OrganisationSearch", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var actualModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(actualModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            actualModel.Compare(expectedModel);
        }

        [Test]
        [Description("Public Sector:Ensure the Organisation type form is confirmed and sent successfully")]
        public void RegisterController_OrganisationType_POST_PublicSector_Success()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = VirtualDateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var expectedModel = new OrganisationViewModel() {
                ManualRegistration = false,
                SectorType = SectorTypes.Public
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(expectedModel);

            //Stash the object for the unstash to happen in code
            controller.StashModel(expectedModel);

            //ACT:
            //2.Run and get the result of the test
            var result = controller.OrganisationType(expectedModel) as RedirectToActionResult;

            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName.ToString() == "OrganisationSearch", "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var actualModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(actualModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            actualModel.Compare(expectedModel);
        }

        #endregion

        #region Organisation Search

        [Test]
        [Description("Ensure the Organisation search form is returned for the current user ")]
        public void RegisterController_OrganisationSearch_GET_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationSearch");
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel() { ManualRegistration = false };
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.OrganisationSearch() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == "OrganisationSearch", "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            // var controller = UiTestHelper.GetController<RegisterController>();
            // controller.PublicSectorRepository.Insert(new EmployerRecord());
        }

        // [Ignore("This test needs fixing")]
        [Test]
        [Description("Ensure that organisation search form has a search text in its field sent successfully and a a matching record is returned")]
        public async Task RegisterController_OrganisationSearch_POST_PrivateSector_SuccessAsync()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = VirtualDateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationSearch");
            routeData.Values.Add("Controller", "Register");

            //search text in model
            var expectedModel = new OrganisationViewModel() {
                Employers = new PagedResult<EmployerRecord>() { },
                SearchText = "smith ltd",
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                CompanyNumber = "456GT657",
                Country = "UK",
                Postcode = "nw1 5re"
            };


            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(expectedModel);

            //insert  some records into the db...
            controller.PrivateSectorRepository.Insert(new EmployerRecord() {
                OrganisationName = "acme inc",
                Address1 = "123",
                Address2 = "EverGreen Terrace",
                CompanyNumber = "123QA432",
                Country = "UK",
                PostCode = "e12 3eq"
            }
                                                     );

            controller.PrivateSectorRepository.Insert(new EmployerRecord() {
                OrganisationName = "smith ltd",
                Address1 = "45",
                Address2 = "iverson rd",
                CompanyNumber = "456GT657",
                Country = "UK",
                PostCode = "nw1 5re"
            }
                                                     );

            controller.PrivateSectorRepository.Insert(new EmployerRecord() {
                OrganisationName = "smith & Wes ltd",
                Address1 = "45",
                Address2 = "iverson rd",
                CompanyNumber = "456GT657",
                Country = "UK",
                PostCode = "nw1 5re"
            }
                                                    );

            controller.PrivateSectorRepository.Insert(new EmployerRecord() {
                OrganisationName = "smithers and sons ltd",
                Address1 = "45",
                Address2 = "iverson rd",
                CompanyNumber = "456GT657",
                Country = "UK",
                PostCode = "nw1 5re"
            }
                                                    );

            controller.PrivateSectorRepository.Insert(new EmployerRecord() {
                OrganisationName = "excetera ltd",
                Address1 = "123",
                Address2 = "Venice avenue ",
                CompanyNumber = "123QA432",
                Country = "UK",
                PostCode = "w1 9eaz"
            }
                                                     );

            //Stash the object for the unstash to happen in code
            controller.StashModel(expectedModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.OrganisationSearch(expectedModel) as RedirectToActionResult;

            //3.If the redirection successfull retrieve the model stash sent with the redirect.
            //returned from the MockPrivateEmployerRepository db then stashed and then unstashed
            var actualModel = controller.UnstashModel<OrganisationViewModel>();

            //check that the search returned a match in the db
            //var sResult     = controller.DataRepository.GetAll<OrganisationViewModel>().Where(o => o.CompanyNumber == resultModel.CompanyNumber);
            //var pagedResult =  controller.PrivateSectorRepository.Search(model.SearchText, 1, Settings.Default.EmployerPageSize);

            //ASSERT:
            //4.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.ChooseOrganisation), "Redirected to the wrong view");

            //5.check that the stashed model with the redirect is not null.
            Assert.NotNull(actualModel, "Expected OrganisationViewModel");

            //check that the model stashed matched what was unstanshed entity wise.
            actualModel.Compare(expectedModel);
        }

        [Test]
        [Description("Ensure the Step4 succeeds when all fields are good")]
        public async Task RegisterController_OrganisationSearch_POST_PublicSector_SuccessAsync()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            //var organisation = new Organisation() { OrganisationId = 1 };
            //var userOrganisation = new UserOrganisation() { OrganisationId = 1, UserId = 1, PINConfirmedDate = VirtualDateTime.Now, PINHash = "0" };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationSearch");
            routeData.Values.Add("Controller", "Register");

            var expectedModel = new OrganisationViewModel() {
                Employers = new PagedResult<EmployerRecord>() { },
                SearchText = "5 Boroughs Partnership NHS Foundation Trust",
                ManualRegistration = false,
                SectorType = SectorTypes.Public
            };


            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);
            controller.Bind(expectedModel);

            //insert  some records into the db...
            controller.PublicSectorRepository.Insert(new EmployerRecord() { OrganisationName = "2Gether NHS Foundation Trust", EmailDomains = "nhs.uk" });
            controller.PublicSectorRepository.Insert(new EmployerRecord() { OrganisationName = "5 Boroughs Partnership NHS Foundation Trust", EmailDomains = "nhs.uk" });
            controller.PublicSectorRepository.Insert(new EmployerRecord() { OrganisationName = "Abbots Langley Parish Council", EmailDomains = "abbotslangley-pc.gov.uk" });
            controller.PublicSectorRepository.Insert(new EmployerRecord() { OrganisationName = "Aberdeen City Council", EmailDomains = "aberdeencityandshire-sdpa.gov.uk" });
            controller.PublicSectorRepository.Insert(new EmployerRecord() { OrganisationName = "Aberdeenshire Council", EmailDomains = "aberdeenshire.gov.uk" });

            //Stash the object for the unstash to happen in code
            controller.StashModel(expectedModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.OrganisationSearch(expectedModel) as RedirectToActionResult;


            //ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.ChooseOrganisation), "Redirected to the wrong view");

            //5.If the redirection successful retrieve the model stash sent with the redirect.
            var actualModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(actualModel, "Expected OrganisationViewModel");

            //7.Verify the values from the result that was stashed matches that of the Arrange values here
            actualModel.Compare(expectedModel);

        }

        #endregion

        #region Choose Organisation

        [Test]
        [Description("Ensure the Choose Organisation form is returned for the current user to choose an organisation")]
        public void RegisterController_ChooseOrganisation_GET_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ChooseOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);
            //controller.StashModel(model);

            var orgModel = new OrganisationViewModel() { ManualRegistration = false };
            controller.StashModel(orgModel);

            //ACT:
            var result = controller.ChooseOrganisation() as ViewResult;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.GetType() == typeof(ViewResult), "Incorrect resultType returned");
            Assert.That(result.ViewName == nameof(RegisterController.ChooseOrganisation), "Incorrect view returned");
            Assert.NotNull(result.Model as OrganisationViewModel, "Expected OrganisationViewModel");
            Assert.That(result.Model.GetType() == typeof(OrganisationViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
        }

        [Test]
        [Description("When public sector organisation has no address redirect to AddAddress")]
        public async Task RegisterController_ChooseOrganisation_POST_PublicOrgNoAddress_RedirectToAddAddressAsync()
        {
            #region ARRANGE

            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>() {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Public  },
                    new EmployerRecord() { OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                    new EmployerRecord() { OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
                }
            };

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 3;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                SearchText = employerResult.Results[selectedEmployerIndex].OrganisationName,
                Employers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Public,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(stashedModel);

            var expectedModel = stashedModel.GetClone();
            expectedModel.ConfirmReturnAction = null;
            expectedModel.AddressReturnAction = nameof(RegisterController.ChooseOrganisation);
            expectedModel.ManualAddress = true;
            expectedModel.ManualRegistration = false;
            expectedModel.SelectedAuthorised = false;
            expectedModel.SelectedEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.ChooseOrganisation(stashedModel, command) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.AddAddress), "Expected Redirect to AddAddress");
            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            #endregion
        }

        [Test]
        [Description("When public sector organisation is authorised with address redirect to ConfirmOrganisation")]
        public async Task RegisterController_ChooseOrganisation_POST_PublicOrgAuthorisedWithAddress_RedirectToConfirmOrganisationAsync()
        {
            #region ARRANGE

            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>() {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { OrganisationName = "Bedford Council",NameSource="CoHo",AddressSource="CoHo",Address1 = "13", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA13", Country = "UK", PostCode = "sw2  5gh", SectorType = SectorTypes.Public,EmailDomains="*@hotmail.com"  },
                    new EmployerRecord() { OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                    new EmployerRecord() { OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
                }
            };
            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 3;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                SearchText = employerResult.Results[selectedEmployerIndex].OrganisationName,
                Employers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Public,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.AddressReturnAction = null;
            expectedModel.ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = false;
            expectedModel.SelectedAuthorised = true;

            expectedModel.SelectedEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.ChooseOrganisation(stashedModel, command) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.ConfirmOrganisation), "Expected Redirect to ConfirmOrganisation");
            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");
            
            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            #endregion
        }

        [Test]
        [Description("When public sector organisation is authorisedwith no address redirect to AddAddress")]
        public async Task RegisterController_ChooseOrganisation_POST_PublicOrgAuthorisedNoAddress_RedirectToAddAddressAsync()
        {
            #region ARRANGE

            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>() {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Public,EmailDomains="*@hotmail.com"  },
                    new EmployerRecord() { OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                    new EmployerRecord() { OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
                }
            };
            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 3;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                SearchText = employerResult.Results[selectedEmployerIndex].OrganisationName,
                Employers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Public,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.ConfirmReturnAction = null;
            expectedModel.AddressReturnAction = nameof(RegisterController.ChooseOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = true;
            expectedModel.SelectedAuthorised = true;
            expectedModel.SelectedEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.ChooseOrganisation(stashedModel, command) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.AddAddress), "Expected Redirect to AddAddress");
            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");
            
            //Check result is same as expected
            expectedModel.Compare(unstashedModel);


            #endregion
        }

        [Test]
        [Description("When private sector organisation has no address redirect to AddAddress")]
        public async Task RegisterController_ChooseOrganisation_POST_PrivateOrgNoAddress_RedirectToAddAddressAsync()
        {
            #region ARRANGE

            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>() {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Private  },
                    new EmployerRecord() { OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                    new EmployerRecord() { OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
                }
            };
            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 3;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                SearchText = employerResult.Results[selectedEmployerIndex].OrganisationName,
                Employers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(stashedModel);


            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.ConfirmReturnAction = null;
            expectedModel.AddressReturnAction = nameof(RegisterController.ChooseOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = true;
            expectedModel.SelectedAuthorised = false;
            expectedModel.SelectedEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.ChooseOrganisation(stashedModel, command) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.AddAddress), "Expected Redirect to AddAddress");
            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            #endregion
        }

        [Test]
        [Description("When 2nd private sector registration and 1st not complete show error message")]
        public async Task RegisterController_ChooseOrganisation_POST_PrivateOrg_2ndRegistration_ShowError()
        {
            #region ARRANGE
            var user1 = new User() { UserId = 1, EmailAddress = "test1@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
                AddressId = 1,
                Status = AddressStatuses.Pending,
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
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation() { OrganisationId = 1, OrganisationName = "Company1", SectorType = SectorTypes.Private, Status = OrganisationStatuses.Pending, CompanyNumber = "12345678" };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic2);

            var userOrg = new UserOrganisation {
                Organisation = org0,
                User = user1,
                PINSentDate = VirtualDateTime.Now,
                Address = address0,
                Method = RegistrationMethods.PinInPost,
            };

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>() {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Private  },
                    new EmployerRecord() { OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                    new EmployerRecord() { OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
                }
            };

            var controller = UiTestHelper.GetController<RegisterController>(user1.UserId, routeData, user1, org0, address0, name, sic1, sic2, userOrg);
            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            foreach (var sicCode in sicCodes)
                controller.DataRepository.Insert(sicCode);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 2;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                SearchText = employerResult.Results[selectedEmployerIndex].OrganisationName,
                Employers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.ConfirmReturnAction = null;
            expectedModel.AddressReturnAction = nameof(RegisterController.ChooseOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = false;
            expectedModel.SelectedAuthorised = false;
            expectedModel.SelectedEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.ChooseOrganisation(stashedModel, command) as ViewResult;
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName==nameof(RegisterController.ChooseOrganisation),"Expected return to same view");
            Assert.That(!result.ViewData.ModelState.IsValid, "Expected ModelState error");
            Assert.That(result.ViewData.ModelState[""].Errors[0].ErrorMessage== "You must complete one registration before you can register more organisations.", "Expected ModelState error");

            #endregion
        }

        [Test]
        [Description("When existing organisation is retired show error message")]
        public async Task RegisterController_ChooseOrganisation_POST_ExistingOrgRetired_ShowError()
        {
            #region ARRANGE
            var user1 = new User() { UserId = 1, EmailAddress = "test1@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
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
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation() { OrganisationId = 1, OrganisationName = "Company1", SectorType = SectorTypes.Private, Status = OrganisationStatuses.Retired, CompanyNumber = "0123QA12" };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic2);

            var userOrg = new UserOrganisation {
                Organisation = org0,
                User = user1,
                PINSentDate = VirtualDateTime.Now,
                PINConfirmedDate=VirtualDateTime.Now,
                Address = address0,
                Method = RegistrationMethods.PinInPost,
            };

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>() {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Private  },
                    new EmployerRecord() { OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                    new EmployerRecord() { OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
                }
            };

            var controller = UiTestHelper.GetController<RegisterController>(user1.UserId, routeData, user1, org0, address0, name, sic1, sic2, userOrg);
            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            foreach (var sicCode in sicCodes)
                controller.DataRepository.Insert(sicCode);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 2;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                SearchText = employerResult.Results[selectedEmployerIndex].OrganisationName,
                Employers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.ConfirmReturnAction = null;
            expectedModel.AddressReturnAction = nameof(RegisterController.ChooseOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = false;
            expectedModel.SelectedAuthorised = false;
            expectedModel.SelectedEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.ChooseOrganisation(stashedModel, command) as ViewResult;
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "CustomError", "Expected custom error page");
            var model = result.Model as ErrorViewModel;
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.ErrorCode==1149, "Wrong error code");
            #endregion
        }

        [Test]
        [Description("When already completed registration for org show error message")]
        public async Task RegisterController_ChooseOrganisation_POST_RegComplete_ShowError()
        {
            #region ARRANGE
            var user1 = new User() { UserId = 1, EmailAddress = "test1@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
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
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation() { OrganisationId = 1, OrganisationName = "Company1", SectorType = SectorTypes.Private, Status = OrganisationStatuses.Active, CompanyNumber = "0123QA12" };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic2);

            var userOrg = new UserOrganisation {
                Organisation = org0,
                User = user1,
                PINSentDate = VirtualDateTime.Now,
                PINConfirmedDate = VirtualDateTime.Now,
                Address = address0,
                Method = RegistrationMethods.PinInPost,
            };

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>() {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Private  },
                    new EmployerRecord() { OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                    new EmployerRecord() { OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
                }
            };

            var controller = UiTestHelper.GetController<RegisterController>(user1.UserId, routeData, user1, org0, address0, name, sic1, sic2, userOrg);
            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            foreach (var sicCode in sicCodes)
                controller.DataRepository.Insert(sicCode);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 2;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                SearchText = employerResult.Results[selectedEmployerIndex].OrganisationName,
                Employers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.ConfirmReturnAction = null;
            expectedModel.AddressReturnAction = nameof(RegisterController.ChooseOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = false;
            expectedModel.SelectedAuthorised = false;
            expectedModel.SelectedEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.ChooseOrganisation(stashedModel, command) as ViewResult;
            #endregion

            #region ASSERT
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == nameof(RegisterController.ChooseOrganisation), "Expected return to same view");
            Assert.That(!result.ViewData.ModelState.IsValid, "Expected ModelState error");
            Assert.That(result.ViewData.ModelState[""].Errors[0].ErrorMessage == "You are already registered for this organisation", "Expected ModelState error");
            #endregion
        }

        [Test]
        [Description("When selected is private and existing is public sector show error")]
        public async Task RegisterController_ChooseOrganisation_POST_SelectedPrivateExistingPublic_ShowError()
        {
            #region ARRANGE
            var user1 = new User() { UserId = 1, EmailAddress = "test1@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
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
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation() { OrganisationId = 1, OrganisationName = "Company1", SectorType = SectorTypes.Public, Status = OrganisationStatuses.Active, CompanyNumber = "0123QA12" };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic2);

            var userOrg = new UserOrganisation {
                Organisation = org0,
                User = user1,
                PINSentDate = VirtualDateTime.Now,
                PINConfirmedDate = VirtualDateTime.Now,
                Address = address0,
                Method = RegistrationMethods.PinInPost,
            };

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>() {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                    new EmployerRecord() { OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Private  },
                    new EmployerRecord() { OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                    new EmployerRecord() { OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
                }
            };

            var controller = UiTestHelper.GetController<RegisterController>(user1.UserId, routeData, user1, org0, address0, name, sic1, sic2, userOrg);
            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            foreach (var sicCode in sicCodes)
                controller.DataRepository.Insert(sicCode);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 2;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                SearchText = employerResult.Results[selectedEmployerIndex].OrganisationName,
                Employers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.ConfirmReturnAction = null;
            expectedModel.AddressReturnAction = nameof(RegisterController.ChooseOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = false;
            expectedModel.SelectedAuthorised = false;
            expectedModel.SelectedEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.ChooseOrganisation(stashedModel, command) as ViewResult;
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "CustomError", "Expected custom error page");
            var model = result.Model as ErrorViewModel;
            Assert.NotNull(model, "Expected ErrorViewModel");
            Assert.That(model.ErrorCode == 1146, "Wrong error code");
            #endregion
        }

        [Test]
        [Description("When private org is pending get orgid from company number then redirect to confirm")]
        public async Task RegisterController_ChooseOrganisation_POST_SelectedPrivatePending_GetIdAndRedirectToConfirm()
        {
            #region ARRANGE
            var user1 = new User() { UserId = 1, EmailAddress = "test1@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var user2 = new User() { UserId = 2, EmailAddress = "test1@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
                AddressId = 1,
                Status = AddressStatuses.Pending,
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
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation() { OrganisationId = 1, OrganisationName = "Company1", SectorType = SectorTypes.Private, Status = OrganisationStatuses.Pending, CompanyNumber = "0123QA12" };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic2);

            var userOrg = new UserOrganisation {
                Organisation = org0,
                User = user2,
                PINSentDate = VirtualDateTime.Now,
                PINConfirmedDate = null,
                Address = address0,
                Method = RegistrationMethods.PinInPost,
            };

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>() {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                    new EmployerRecord() { OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                    new EmployerRecord() { OrganisationId = 1, OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de", SectorType = SectorTypes.Private },
                    new EmployerRecord() { OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Private  },
                    new EmployerRecord() { OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                    new EmployerRecord() { OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                    new EmployerRecord() { OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                    new EmployerRecord() { OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                    new EmployerRecord() { OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                    new EmployerRecord() { OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                    new EmployerRecord() { OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                    new EmployerRecord() { OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                    new EmployerRecord() { OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                    new EmployerRecord() { OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                    new EmployerRecord() { OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
                }
            };

            var controller = UiTestHelper.GetController<RegisterController>(user1.UserId, routeData, user1, user2, org0, address0, name, sic1, sic2, userOrg);
            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            foreach (var sicCode in sicCodes)
                controller.DataRepository.Insert(sicCode);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 2;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                SearchText = employerResult.Results[selectedEmployerIndex].OrganisationName,
                Employers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                SicCodes = new List<int>(),
                ManualEmployers = new List<EmployerRecord>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.AddressReturnAction = null;
            expectedModel.ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = false;
            expectedModel.SelectedAuthorised = false;
            expectedModel.SelectedEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.ChooseOrganisation(stashedModel, command) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.AreEqual(nameof(RegisterController.ConfirmOrganisation), result.ActionName, "Expected Redirect to ConfirmOrganisation");

            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            Assert.That(unstashedModel.GetSelectedEmployer().OrganisationId==org0.OrganisationId,"Incorrect OrganisationId");
            #endregion
        }

        #endregion

        #region Add Organisation
        /// <summary>
        /// TODO: Review, as this tests might be a duplicate
        /// </summary>
        [Test]
        [Description("Ensure the AddOrganisation form is returned correctly")]
        public void RegisterController_ChooseOrganisation_GET_Form_Is_Returned_Correctly()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            var orgModel = new OrganisationViewModel() {
                ManualRegistration = true,
                SearchText = "Test Co"
            };

            controller.StashModel(orgModel);

            //ACT:
            var result = controller.AddOrganisation() as ViewResult;
            var model = result?.Model as OrganisationViewModel;
            var stashedModel = controller.UnstashModel<OrganisationViewModel>();

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == nameof(RegisterController.AddOrganisation), "Expected Viewname=AddOrganisation");
            Assert.NotNull(model, "Expected model of OrganisationViewModel");
            Assert.NotNull(stashedModel, "Expected model saved to stash");
            Assert.That(model.ManualRegistration == true, "Expected ManualAddress to be false");
            Assert.That(model.OrganisationName == model.SearchText, "Expected OrganisationName to be SearchText");
        }

        [Test]
        [Description("Ensure that the AddOrganisation form values are saved correctly")]
        public async Task RegisterController_ChooseOrganisation_POST_NoReferences_RedirectToAddAddressAsync()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            //use the include and exclude functions here to save typing
            var model = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = null,
                CharityNumber = null,
                MutualNumber = null,
                OtherName = null,
                OtherValue = null,
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
                ManualEmployers = null,
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation),
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            var savedModel = model.GetClone();
            savedModel.NoReference = true;
            savedModel.OrganisationName = "XYZ Ltd";
            controller.Bind(savedModel);
            var expectedModel = savedModel.GetClone();
            expectedModel.ManualRegistration = true;
            expectedModel.ManualAddress = false;
            expectedModel.ManualAuthorised = false;
            expectedModel.NameSource = user.EmailAddress;
            expectedModel.AddressReturnAction = nameof(RegisterController.AddOrganisation);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.AddOrganisation(savedModel) as RedirectToActionResult;

            //ASSERTS:
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.AddAddress), "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unstashedModel, "Expected OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

        }

        [Test]
        [Description("Ensure when existing company number entered redireced to ConfirmOrganisation page")]
        public async Task RegisterController_ChooseOrganisation_POST_ExistingCompanyNo_RedirectToConfirmOrganisationAsync()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address = new OrganisationAddress() { AddressId = 1, OrganisationId = 1, Address1 = "Address line 1", PostCode = "PC1", Status = AddressStatuses.Active, Source = "CoHo" };
            var addressStatus = new AddressStatus { Address = address, Status = AddressStatuses.Active };
            address.AddressStatuses.Add(addressStatus);
            var org = new Organisation()
            {
                OrganisationId = 1,
                CompanyNumber = "11223344",
                SectorType = SectorTypes.Private,
                OrganisationName = "ORIGINAL NAME",
                OrganisationAddresses = new List<OrganisationAddress> { address },
                Status = OrganisationStatuses.Active
            };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            //use the include and exclude functions here to save typing
            var model = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "",
                CharityNumber = null,
                MutualNumber = null,
                OtherName = null,
                OtherValue = null,
                DateOfCessation = VirtualDateTime.Now,

                ManualAddress = false,

                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactPhoneNumber = "79000 000 000",

                SicCodeIds = null,
                SicSource = "CoHo",

                SearchText = "Searchtext",
                ManualRegistration = true,
                SectorType = SectorTypes.Private,
                PINExpired = false,
                PINSent = false,
                SelectedEmployerIndex = 0,
                Employers = employerResult, //0 record returned
                ManualEmployers = null,
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation),
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user, org, address);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            var savedModel = model.GetClone();
            savedModel.NoReference = false;
            savedModel.CompanyNumber = "11223344";
            savedModel.OrganisationName = "XYZ Ltd";
            controller.Bind(savedModel);

            //Set the expected data model
            var expectedModel = savedModel.GetClone();
            expectedModel.ManualEmployerIndex = 0;
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = false;
            expectedModel.ManualAuthorised = false;
            expectedModel.ConfirmReturnAction = nameof(RegisterController.AddOrganisation);
            expectedModel.MatchedReferenceCount = 1;
            expectedModel.NameSource = user.EmailAddress;
            expectedModel.ManualEmployers = new List<EmployerRecord>() { org.ToEmployerRecord() };
            expectedModel.ContactEmailAddress = null;
            expectedModel.ContactFirstName = null;
            expectedModel.ContactLastName = null;
            expectedModel.ContactJobTitle = null;
            expectedModel.ContactPhoneNumber = null;

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.AddOrganisation(savedModel) as RedirectToActionResult;

            //ASSERTS:
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.AreEqual(result.ActionName.ToString(), nameof(RegisterController.ConfirmOrganisation), "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unstashedModel, "Expected OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

        }

        [Test]
        [Description("Ensure when not authorized public sector org redirect to AddAddress")]
        public async Task RegisterController_ChooseOrganisation_POST_ExistingCompanyNoAddress_RedirectToAddAddressAsync()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var org = new Organisation() { OrganisationId = 1, CompanyNumber = "11223344", SectorType = SectorTypes.Public, OrganisationName = "ORIGINAL NAME", Status= OrganisationStatuses.Active };

            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            //use the include and exclude functions here to save typing
            var model = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "",
                CharityNumber = null,
                MutualNumber = null,
                OtherName = null,
                OtherValue = null,
                DateOfCessation = VirtualDateTime.Now,

                ManualAddress = false,

                ContactEmailAddress = "test@hotmail.com",
                ContactFirstName = "test firstName",
                ContactLastName = "test lastName",
                ContactJobTitle = "test job title",
                ContactPhoneNumber = "79000 000 000",

                SicCodeIds = "1,2,3,4",
                SicSource = "CoHo",

                SearchText = "Searchtext",
                ManualRegistration = true,
                SectorType = SectorTypes.Private,
                PINExpired = false,
                PINSent = false,
                SelectedEmployerIndex = 0,
                Employers = employerResult, //0 record returned
                ManualEmployers = null,
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation),
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user, org);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            var savedModel = model.GetClone();
            savedModel.NoReference = false;
            savedModel.CompanyNumber = "11223344";
            savedModel.OrganisationName = "XYZ Ltd";
            controller.Bind(savedModel);

            //Set the expected data model
            var expectedModel = savedModel.GetClone();
            expectedModel.ManualEmployerIndex = 0;
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = true;
            expectedModel.ManualAuthorised = false;
            expectedModel.AddressReturnAction = nameof(RegisterController.AddOrganisation);
            expectedModel.MatchedReferenceCount = 1;
            expectedModel.NameSource = user.EmailAddress;
            expectedModel.ManualEmployers = new List<EmployerRecord>() { org.ToEmployerRecord() };

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.AddOrganisation(savedModel) as RedirectToActionResult;

            //ASSERTS:
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.AddAddress), "Redirected to the wrong view");

            //5.If the redirection successful retrieve the model stash sent with the redirect.
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unstashedModel, "Expected OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

        }

        [Test]
        [Description("Ensure that the AddOrganisation redirects correctly to select organisation when 2 matching references")]
        public async Task RegisterController_ChooseOrganisation_POST_2MatchingReferences_RedirectToSelectOrganisationAsync()
        {
            //ARRANGE:
            //1.Arrange the test setup variables
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address1 = new OrganisationAddress() { AddressId = 1, OrganisationId = 1, Address1 = "Address line 1", PostCode = "PC1", Status = AddressStatuses.Active, Source = "CoHo" };
            var org1 = new Organisation()
            {
                OrganisationId = 1,
                CompanyNumber = "11223344",
                SectorType = SectorTypes.Private,
                OrganisationName = "ORIGINAL NAME",
                OrganisationAddresses = new List<OrganisationAddress> { address1 }
            };
            var address2 = new OrganisationAddress() { AddressId = 2, OrganisationId = 2, Address1 = "Address line 1a", PostCode = "PC1a", Status = AddressStatuses.Active, Source = "CoHo" };
            var org2 = new Organisation()
            {
                OrganisationId = 2,
                SectorType = SectorTypes.Private,
                OrganisationName = "ORIGINAL NAMEa",
                OrganisationAddresses = new List<OrganisationAddress> { address2 }
            };
            var reference = new OrganisationReference() {
                OrganisationReferenceId = 1,
                OrganisationId = 2,
                ReferenceName = nameof(OrganisationViewModel.CharityNumber),
                ReferenceValue = "CharityNo1"
            };

            org2.OrganisationReferences.Add(reference);
            //Set user email address verified code and expired sent date
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new PagedResult<EmployerRecord>();
            employerResult.Results = new List<EmployerRecord>();

            //use the include and exclude functions here to save typing
            var model = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "",
                CharityNumber = null,
                MutualNumber = null,
                OtherName = null,
                OtherValue = null,
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
                ManualRegistration = true,
                SectorType = SectorTypes.Private,
                PINExpired = false,
                PINSent = false,
                SelectedEmployerIndex = 0,
                Employers = employerResult, //0 record returned
                ManualEmployers = null,
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation),
            };

            var controller = UiTestHelper.GetController<RegisterController>(1, routeData, user, org1, address1, org2, address2, reference);

            //Stash the object for the unstash to happen in code
            controller.StashModel(model);

            var savedModel = model.GetClone();
            savedModel.NoReference = false;
            savedModel.CompanyNumber = "11223344";
            savedModel.CharityNumber = "CharityNo1";
            savedModel.OrganisationName = "XYZ Ltd";
            controller.Bind(savedModel);

            //Set the expected data model
            var expectedModel = savedModel.GetClone();
            expectedModel.ManualEmployerIndex = -1;
            expectedModel.MatchedReferenceCount = 2;
            expectedModel.NameSource = user.EmailAddress;
            expectedModel.ManualEmployers = new List<EmployerRecord>() { org1.ToEmployerRecord(), org2.ToEmployerRecord() };

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.AddOrganisation(savedModel) as RedirectToActionResult;

            //ASSERTS:
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.SelectOrganisation), "Redirected to the wrong view");

            //5.If the redirection successfull retrieve the model stash sent with the redirect.
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //6.Check that the unstashed model is not null
            Assert.NotNull(unstashedModel, "Expected OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

        }

        #endregion

        #region Select Organisation

        [Test]
        [Description("Ensure the SelectOrganisation form is returned correctly")]
        public void RegisterController_SelectOrganisation_GET_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.SelectOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            var orgModel = new OrganisationViewModel() {
                ManualRegistration = false,
                ManualAddress = true,
                SectorType = SectorTypes.Public
            };

            controller.StashModel(orgModel);

            //ACT:
            var result = controller.SelectOrganisation() as ViewResult;
            var model = result?.Model as OrganisationViewModel;
            var stashedModel = controller.UnstashModel<OrganisationViewModel>();

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == nameof(RegisterController.SelectOrganisation), "Expected Viewname=SelectOrganisation");
            Assert.NotNull(model, "Expected model of OrganisationViewModel");
            Assert.NotNull(stashedModel, "Expected model saved to stash");
        }

        [Test]
        [Description("When public sector organisation has no address redirect to AddAddress")]
        public async Task RegisterController_SelectOrganisation_POST_PublicOrgNoAddress_RedirectToAddAddress()
        {
            #region ARRANGE

            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new List<EmployerRecord>()
            {
                new EmployerRecord() {OrganisationId=1, OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                new EmployerRecord() {OrganisationId=2, OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                new EmployerRecord() {OrganisationId=3, OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                new EmployerRecord() {OrganisationId=4, OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Public  },
                new EmployerRecord() {OrganisationId=5, OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                new EmployerRecord() {OrganisationId=6, OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                new EmployerRecord() {OrganisationId=7, OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                new EmployerRecord() {OrganisationId=8, OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                new EmployerRecord() {OrganisationId=9, OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                new EmployerRecord() {OrganisationId=10, OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                new EmployerRecord() {OrganisationId=11, OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                new EmployerRecord() {OrganisationId=12, OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                new EmployerRecord() {OrganisationId=13, OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                new EmployerRecord() {OrganisationId=14, OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                new EmployerRecord() {OrganisationId=15, OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
            };

            var orgs = new List<Organisation>();
            var addresses = new List<OrganisationAddress>();
            //Create the org and address
            foreach (var employer in employerResult)
            {
                var address = new OrganisationAddress() {
                    Address1 = employer.Address1,
                    Address2 = employer.Address2,
                    Address3 = employer.Address3,
                    TownCity = employer.City,
                    County = employer.County,
                    Country = employer.Country,
                    PostCode = employer.PostCode,
                    PoBox = employer.PoBox,
                };
                var org = new Organisation() {
                    OrganisationId = employer.OrganisationId,
                    OrganisationName = employer.OrganisationName,
                    SectorType = employer.SectorType,
                    CompanyNumber = employer.CompanyNumber,
                    Status = OrganisationStatuses.Active
                };
                if (!string.IsNullOrWhiteSpace(address.GetAddressString()))
                {
                    addresses.Add(address);
                }

                orgs.Add(org);
            }

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, orgs, addresses);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 3;
            string command = $"employer_{selectedEmployerIndex}";

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                ManualEmployers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                CompanyNumber = "1234589",
                CharityNumber = "ABCDEFG",
                MutualNumber = "9876543",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                OrganisationName = "TEST ORG 123",
                NameSource = user.EmailAddress,
                SicCodes = new List<int>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.AddressReturnAction = nameof(RegisterController.SelectOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = true;
            expectedModel.ManualEmployerIndex = selectedEmployerIndex;
            #endregion


            #region ACT
            var result = await controller.SelectOrganisation(command) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.AddAddress), "Expected Redirect to AddAddress");
            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);


            #endregion
        }

        [Test]
        [Description("When public sector organisation is authorised with address redirect to ConfirmOrganisation")]
        public async Task RegisterController_SelectOrganisation_POST_PublicOrgAuthorisedWithAddress_RedirectToConfirmOrganisationAsync()
        {
            #region ARRANGE

            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new List<EmployerRecord>()
            {
                new EmployerRecord() {OrganisationId=1, OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                new EmployerRecord() {OrganisationId=2, OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                new EmployerRecord() {OrganisationId=3, OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                new EmployerRecord() {OrganisationId=4, OrganisationName = "Bedford Council",NameSource="CoHo",AddressSource="CoHo",Address1 = "13", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA13", Country = "UK", PostCode = "sw2  5gh", SectorType = SectorTypes.Public,EmailDomains="*@hotmail.com"  },
                new EmployerRecord() {OrganisationId=5, OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                new EmployerRecord() {OrganisationId=6, OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                new EmployerRecord() {OrganisationId=7, OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                new EmployerRecord() {OrganisationId=8, OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                new EmployerRecord() {OrganisationId=9, OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                new EmployerRecord() {OrganisationId=10, OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                new EmployerRecord() {OrganisationId=11, OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                new EmployerRecord() {OrganisationId=12, OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                new EmployerRecord() {OrganisationId=13, OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                new EmployerRecord() {OrganisationId=14, OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                new EmployerRecord() {OrganisationId=15, OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
            };

            var orgs = new List<Organisation>();
            var addresses = new List<OrganisationAddress>();
            //Create the org and address
            long addressId = 0;
            foreach (var employer in employerResult)
            {
                var address = new OrganisationAddress() {
                    AddressId = ++addressId,
                    Address1 = employer.Address1,
                    Address2 = employer.Address2,
                    Address3 = employer.Address3,
                    TownCity = employer.City,
                    County = employer.County,
                    Country = employer.Country,
                    PostCode = employer.PostCode,
                    PoBox = employer.PoBox,
                };
                var org1 = new Organisation() {
                    OrganisationId = employer.OrganisationId,
                    OrganisationName = employer.OrganisationName,
                    SectorType = employer.SectorType,
                    CompanyNumber = employer.CompanyNumber,
                    Status = OrganisationStatuses.Active
                };
                if (!string.IsNullOrWhiteSpace(address.GetAddressString()))
                {
                    addresses.Add(address);
                }
                orgs.Add(org1);
            }

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, orgs, addresses);

            //change recordNum to test each record: 
            var selectedEmployerIndex = 3;
            string command = $"employer_{selectedEmployerIndex}";

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                ManualEmployers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                CompanyNumber = "1234589",
                CharityNumber = "ABCDEFG",
                MutualNumber = "9876543",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                OrganisationName = "TEST ORG 123",
                NameSource = user.EmailAddress,
                SicCodes = new List<int>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.ManualEmployerIndex = selectedEmployerIndex;

            expectedModel.ConfirmReturnAction = nameof(RegisterController.SelectOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = false;
            expectedModel.ManualAuthorised = true;
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == expectedModel.GetManualEmployer().OrganisationId);
            #endregion


            #region ACT
            var result = await controller.SelectOrganisation(command) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.ConfirmOrganisation), "Expected Redirect to ConfirmOrganisation");
            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);


            #endregion
        }

        [Test]
        [Description("When public sector organisation is authorised with no address redirect to AddAddress")]
        public async Task RegisterController_SelectOrganisation_POST_PublicOrgAuthorisedNoAddress_RedirectToAddAddressAsync()
        {
            #region ARRANGE

            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new List<EmployerRecord>()
            {
                new EmployerRecord() {OrganisationId=1, OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                new EmployerRecord() {OrganisationId=2, OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                new EmployerRecord() {OrganisationId=3, OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                new EmployerRecord() {OrganisationId=4, OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Public,EmailDomains="*@hotmail.com"  },
                new EmployerRecord() {OrganisationId=5, OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                new EmployerRecord() {OrganisationId=6, OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                new EmployerRecord() {OrganisationId=7, OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                new EmployerRecord() {OrganisationId=8, OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                new EmployerRecord() {OrganisationId=9, OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                new EmployerRecord() {OrganisationId=10, OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                new EmployerRecord() {OrganisationId=11, OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                new EmployerRecord() {OrganisationId=12, OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                new EmployerRecord() {OrganisationId=13, OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                new EmployerRecord() {OrganisationId=14, OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                new EmployerRecord() {OrganisationId=15, OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
            };

            //Create the org and address
            var orgs = new List<Organisation>();
            var addresses = new List<OrganisationAddress>();
            foreach (var employer in employerResult)
            {
                var address = new OrganisationAddress() {
                    Address1 = employer.Address1,
                    Address2 = employer.Address2,
                    Address3 = employer.Address3,
                    TownCity = employer.City,
                    County = employer.County,
                    Country = employer.Country,
                    PostCode = employer.PostCode,
                    PoBox = employer.PoBox,
                };
                var org = new Organisation() {
                    OrganisationId = employer.OrganisationId,
                    OrganisationName = employer.OrganisationName,
                    SectorType = employer.SectorType,
                    CompanyNumber = employer.CompanyNumber,
                    Status = OrganisationStatuses.Active
                };
                if (!string.IsNullOrWhiteSpace(address.GetAddressString()))
                {
                    addresses.Add(address);
                }
                orgs.Add(org);
            }

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, orgs, addresses);
          

            //change recordNum to test each record: 
            var selectedEmployerIndex = 3;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                ManualEmployers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Public,
                CompanyNumber = "1234589",
                CharityNumber = "ABCDEFG",
                MutualNumber = "9876543",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                OrganisationName = "TEST ORG 123",
                NameSource = user.EmailAddress,
                SicCodes = new List<int>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.AddressReturnAction = nameof(RegisterController.SelectOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = true;
            expectedModel.ManualAuthorised = true;
            expectedModel.ManualEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.SelectOrganisation(command) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.AddAddress), "Expected Redirect to AddAddress");
            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);


            #endregion
        }

        [Test]
        [Description("When private sector organisation has no address redirect to AddAddress")]
        public async Task RegisterController_SelectOrganisation_POST_PrivateOrgNoAddress_RedirectToAddAddressAsync()
        {
            #region ARRANGE

            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new List<EmployerRecord>()
            {
                new EmployerRecord() {OrganisationId=1, OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                new EmployerRecord() {OrganisationId=2, OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                new EmployerRecord() {OrganisationId=3, OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                new EmployerRecord() {OrganisationId=4, OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Private  },
                new EmployerRecord() {OrganisationId=5, OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                new EmployerRecord() {OrganisationId=6, OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                new EmployerRecord() {OrganisationId=7, OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                new EmployerRecord() {OrganisationId=8, OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                new EmployerRecord() {OrganisationId=9, OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                new EmployerRecord() {OrganisationId=10, OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                new EmployerRecord() {OrganisationId=11, OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                new EmployerRecord() {OrganisationId=12, OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                new EmployerRecord() {OrganisationId=13, OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                new EmployerRecord() {OrganisationId=14, OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                new EmployerRecord() {OrganisationId=15, OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
            };

            //Create the org and address
            var orgs = new List<Organisation>();
            var addresses = new List<OrganisationAddress>();
            foreach (var employer in employerResult)
            {
                var address = new OrganisationAddress() {
                    Address1 = employer.Address1,
                    Address2 = employer.Address2,
                    Address3 = employer.Address3,
                    TownCity = employer.City,
                    County = employer.County,
                    Country = employer.Country,
                    PostCode = employer.PostCode,
                    PoBox = employer.PoBox,
                };
                var org = new Organisation() {
                    OrganisationId = employer.OrganisationId,
                    OrganisationName = employer.OrganisationName,
                    SectorType = employer.SectorType,
                    CompanyNumber = employer.CompanyNumber,
                    Status = OrganisationStatuses.Active
                };
                if (!string.IsNullOrWhiteSpace(address.GetAddressString()))
                {
                    addresses.Add(address);
                }
                orgs.Add(org);
            }

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, orgs, addresses);

            //Set the initial model
            var selectedEmployerIndex = 3;
            string command = $"employer_{selectedEmployerIndex}";

            var stashedModel = new OrganisationViewModel() {
                ManualEmployers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                CompanyNumber = "1234589",
                CharityNumber = "ABCDEFG",
                MutualNumber = "9876543",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                OrganisationName = "TEST ORG 123",
                NameSource = user.EmailAddress,
                SicCodes = new List<int>()
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.AddressReturnAction = nameof(RegisterController.SelectOrganisation);
            expectedModel.ManualRegistration = false;
            expectedModel.ManualAddress = true;
            expectedModel.ManualAuthorised = false;
            expectedModel.ManualEmployerIndex = selectedEmployerIndex;

            #endregion


            #region ACT
            var result = await controller.SelectOrganisation(command) as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.AddAddress), "Expected Redirect to AddAddress");
            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            #endregion
        }

        [Test]
        [Description("When no organisation selected redirect to AddAddress")]
        public async Task RegisterController_SelectOrganisation_POST_Continue_RedirectToAddAddressAsync()
        {
            #region ARRANGE

            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.AddOrganisation));
            routeData.Values.Add("Controller", "Register");

            var employerResult = new List<EmployerRecord>()
            {
                new EmployerRecord() {OrganisationId=1, OrganisationName = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA10", Country = "UK", PostCode = "w12  3we" },
                new EmployerRecord() {OrganisationId=2, OrganisationName = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA11", Country = "UK", PostCode = "n12  4qw" },
                new EmployerRecord() {OrganisationId=3, OrganisationName = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA12", Country = "UK", PostCode = "nw2  1de" },
                new EmployerRecord() {OrganisationId=4, OrganisationName = "Bedford Council",NameSource="CoHo",SectorType = SectorTypes.Private  },
                new EmployerRecord() {OrganisationId=5, OrganisationName = "Exeter Council", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA14", Country = "UK", PostCode = "se2  2bh",SectorType = SectorTypes.Public },
                new EmployerRecord() {OrganisationId=6, OrganisationName = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA15", Country = "UK", PostCode = "da2  6cd" },
                new EmployerRecord() {OrganisationId=7, OrganisationName = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA16", Country = "UK", PostCode = "cd2  1cs" },
                new EmployerRecord() {OrganisationId=8, OrganisationName = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA17", Country = "UK", PostCode = "e12  7xs" },
                new EmployerRecord() {OrganisationId=9, OrganisationName = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA18", Country = "UK", PostCode = "e17  8za" },
                new EmployerRecord() {OrganisationId=10, OrganisationName = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA19", Country = "UK", PostCode = "sw25 9bh" },
                new EmployerRecord() {OrganisationId=11, OrganisationName = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA20", Country = "UK", PostCode = "se1  6nh" },
                new EmployerRecord() {OrganisationId=12, OrganisationName = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA21", Country = "UK", PostCode = "se32 2nj" },
                new EmployerRecord() {OrganisationId=13, OrganisationName = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA22", Country = "UK", PostCode = "e1   1nh" },
                new EmployerRecord() {OrganisationId=14, OrganisationName = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA23", Country = "UK", PostCode = "e19  8vt" },
                new EmployerRecord() {OrganisationId=15, OrganisationName = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "0123QA24", Country = "UK", PostCode = "sw1  5ml" },
            };

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                ManualEmployers = employerResult,
                ManualRegistration = false,
                SectorType = SectorTypes.Private,
                CompanyNumber = "1234589",
                CharityNumber = "ABCDEFG",
                MutualNumber = "9876543",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                OrganisationName = "TEST ORG 123",
                NameSource = user.EmailAddress,
                SicCodes = new List<int>()
            };

            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.ConfirmReturnAction = null;
            expectedModel.AddressReturnAction = nameof(RegisterController.SelectOrganisation);
            expectedModel.ManualRegistration = true;
            expectedModel.ManualAddress = false;
            expectedModel.ManualAuthorised = false;
            expectedModel.ManualEmployerIndex = -1;

            #endregion


            #region ACT
            var result = await controller.SelectOrganisation("Continue") as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            #endregion


            #region ASSERT
            Assert.NotNull(result, "Expected RedirectResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.AddAddress), "Expected Redirect to AddAddress");
            Assert.NotNull(unstashedModel, "No stashed OrganisationViewModel", "No model was stashed");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            #endregion
        }

        #endregion

        #region Confirm Organisation

        [Test]
        [Description("Ensure the ConfirmOrganisation returns SIC Codes for manual registration SicCodeIds")]
        public async Task RegisterController_ConfirmOrganisation_GET_ManualRegistration_AddSicCodesAsync()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var sicCode0 = new SicCode() { SicCodeId = 1, Description = "Public Sector", SicSection = new SicSection() { SicSectionId = "X", Description = "Public Sector" } };
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };

            var sicCodes = new List<SicCode>() {
                sicCode1,
                sicCode2
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");


            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, sicCode0, sicCodes);


            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = null,
                SectorType = SectorTypes.Public,

                AddressSource = user.EmailAddress,
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

                SicCodeIds = "10520,2100",
                SicSource = user.EmailAddress,

                SearchText = null,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = true,
                PINExpired = false,
                PINSent = false,
                Employers = null,
                SelectedEmployerIndex = -1,
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.AddOrganisation),
                ConfirmReturnAction = nameof(RegisterController.AddSector),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            sicCodes.Add(sicCode0);
            expectedModel.SicCodes = sicCodes.OrderBy(s => s.SicCodeId).Select(s=>s.SicCodeId).ToList();

            //ACT:
            var result = await controller.ConfirmOrganisation() as ViewResult;
            var model = result?.Model as OrganisationViewModel;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == nameof(RegisterController.ConfirmOrganisation), "Incorrect view returned");
            Assert.NotNull(result.Model, "Expected OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

        }

        [Test]
        [Description("Ensure the ConfirmOrganisation returns SIC Codes for private organisations from companies house")]
        public async Task RegisterController_ConfirmOrganisation_GET_PrivateNotManualRegistration_AddSicCodesAsync()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var sicCode0 = new SicCode() { SicCodeId = 1, Description = "Public Sector", SicSection = new SicSection() { SicSectionId = "X", Description = "Public Sector" } };
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };

            var sicCodes = new List<SicCode>() {
                sicCode1,
                sicCode2
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, sicCode0, sicCodes);

            var employer = new EmployerRecord() {
                OrganisationName = "Company1",
                SectorType = SectorTypes.Private,
                CompanyNumber = "12345678",
                SicCodeIds = "2100,10520",
                SicSource = "CoHo"
            };
            controller.PrivateSectorRepository.Insert(employer.GetClone());

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = null,
                NameSource = null,
                CompanyNumber = null,
                CharityNumber = null,
                MutualNumber = null,
                OtherName = null,
                OtherValue = null,
                DateOfCessation = null,
                SectorType = SectorTypes.Private,

                AddressSource = null,
                Address1 = null,
                Address2 = null,
                Address3 = null,
                City = null,
                County = null,
                Country = null,
                PoBox = null,
                Postcode = null,

                ContactEmailAddress = null,
                ContactFirstName = null,
                ContactLastName = null,
                ContactJobTitle = null,
                ContactPhoneNumber = null,

                SicCodeIds = null,
                SicSource = null,

                SearchText = null,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = false,
                PINExpired = false,
                PINSent = false,
                Employers = null,
                SelectedEmployerIndex = -1,
                ManualEmployers = new List<EmployerRecord>() { employer },
                ManualEmployerIndex = 0,
                AddressReturnAction = nameof(RegisterController.AddOrganisation),
                ConfirmReturnAction = nameof(RegisterController.AddSector),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.OrganisationName = employer.OrganisationName;
            expectedModel.CompanyNumber = employer.CompanyNumber;
            expectedModel.SicCodeIds = employer.SicCodeIds;
            expectedModel.SicSource = employer.SicSource;
            expectedModel.SicCodes = sicCodes.Select(s => s.SicCodeId).ToList();

            //ACT:
            var result = await controller.ConfirmOrganisation() as ViewResult;
            var viewModel = result?.Model as OrganisationViewModel;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == nameof(RegisterController.ConfirmOrganisation), "Incorrect view returned");
            Assert.NotNull(result.Model, "Expected OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(viewModel);

        }

        [Test]
        [Description("Ensure the ConfirmOrganisation returns SIC Codes for public organisations from companies house")]
        public async Task RegisterController_ConfirmOrganisation_GET_PublicNotManualRegistration_AddSicCodesAsync()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var sicCode0 = new SicCode() { SicCodeId = 1, Description = "Public Sector", SicSection = new SicSection() { SicSectionId = "X", Description = "Public Sector" } };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user);

            var sicCodes = new List<SicCode>() {
                sicCode0,
                new SicCode() { SicCodeId = 2100 },
                new SicCode() { SicCodeId = 10520 },
            };

            foreach (var sicCode in sicCodes)
                controller.DataRepository.Insert(sicCode);

            await controller.DataRepository.SaveChangesAsync();

            var employer = new EmployerRecord() {
                OrganisationName = "Company1",
                SectorType = SectorTypes.Public,
                CompanyNumber = "12345678",
                SicCodeIds = "2100,10520",
                SicSource = "CoHo"
            };
            controller.PublicSectorRepository.Insert(employer.GetClone());
            employer.SicCodeIds = null;

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = null,
                NameSource = null,
                CompanyNumber = null,
                CharityNumber = null,
                MutualNumber = null,
                OtherName = null,
                OtherValue = null,
                DateOfCessation = null,
                SectorType = SectorTypes.Public,

                AddressSource = null,
                Address1 = null,
                Address2 = null,
                Address3 = null,
                City = null,
                County = null,
                Country = null,
                PoBox = null,
                Postcode = null,

                ContactEmailAddress = null,
                ContactFirstName = null,
                ContactLastName = null,
                ContactJobTitle = null,
                ContactPhoneNumber = null,

                SicCodeIds = null,
                SicSource = null,

                SearchText = null,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = false,
                PINExpired = false,
                PINSent = false,
                Employers = new PagedResult<EmployerRecord>() {
                    Results = new List<EmployerRecord>() { employer }
                },
                SelectedEmployerIndex = 0,
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = 0,
                AddressReturnAction = nameof(RegisterController.AddOrganisation),
                ConfirmReturnAction = nameof(RegisterController.AddSector),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.OrganisationName = employer.OrganisationName;
            expectedModel.CompanyNumber = employer.CompanyNumber;
            expectedModel.SicCodeIds = "1,2100,10520";
            expectedModel.Employers.Results[0].SicCodeIds = expectedModel.SicCodeIds;
            expectedModel.Employers.Results[0].SicSource = employer.SicSource;
            expectedModel.SicCodes = sicCodes.Select(s => s.SicCodeId).ToList();
            expectedModel.SicSource = employer.SicSource;

            //ACT:
            var result = await controller.ConfirmOrganisation() as ViewResult;
            var viewModel = result?.Model as OrganisationViewModel;

            //ASSERT:
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == nameof(RegisterController.ConfirmOrganisation), "Incorrect view returned");
            Assert.NotNull(result.Model, "Expected OrganisationViewModel");

            //Check result is same as expected
            expectedModel.Compare(viewModel);

        }

        [Test]
        [Description("Ensure the ConfirmOrganisation saves new private sector org from CoHo")]
        public async Task RegisterController_ConfirmOrganisation_POST_NewPrivateSectorWithAddress_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };

            var sicCodes = new List<SicCode>() {
                sicCode1,
                sicCode2
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, sicCodes);

            var employer = new EmployerRecord() {
                OrganisationName = "Company1",
                SectorType = SectorTypes.Private,
                CompanyNumber = "12345678",
                SicCodeIds = "2100,10520",
                Address1 = "123",
                Address2 = "EverGreen Terrace",
                Country = "UK",
                PostCode = "e12 3eq"
            };
            controller.PrivateSectorRepository.Insert(employer);
            var currentSnapshotDate = employer.SectorType.GetAccountingStartDate();

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                SectorType = SectorTypes.Private,

                AddressSource = null,
                Address1 = null,
                Address2 = null,
                Address3 = null,
                City = null,
                County = null,
                Country = null,
                PoBox = null,
                Postcode = null,
                IsUkAddress = true,

                ContactEmailAddress = null,
                ContactFirstName = null,
                ContactLastName = null,
                ContactJobTitle = null,
                ContactPhoneNumber = null,

                SicCodeIds = employer.SicCodeIds,
                SicCodes = sicCodes.Select(s => s.SicCodeId).ToList(),
                SicSource = "CoHo",

                SearchText = null,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = false,
                PINExpired = false,
                PINSent = false,
                Employers = null,
                SelectedEmployerIndex = -1,
                ManualEmployers = new List<EmployerRecord>() { employer },
                ManualEmployerIndex = 0,
                AddressReturnAction = null,
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == employer.OrganisationName);
            var userOrgs = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration))
            {
                Assert.That(result.ActionName == nameof(RegisterController.RequestReceived), "Redirected to the wrong view");
            }
            else
            {
                Assert.That(result.ActionName == nameof(RegisterController.PINSent), "Redirected to the wrong view");
            }
            
            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            Assert.NotNull(org, "No organisation saved");
            Assert.That(userOrgs != null && userOrgs.Any(), "No user organisation saved");
            Assert.That(addresses != null && addresses.Any(), "No organisation address saved");
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Any(), "No organisation names saved");
            Assert.That(!string.IsNullOrWhiteSpace(org.EmployerReference), "Expected new EmployerReferenceto be  assigned");
            Assert.That(org.OrganisationScopes.Count == 2 && org.OrganisationScopes.Any(os => os.Status == ScopeRowStatuses.Active && os.ScopeStatus == ScopeStatuses.PresumedInScope && os.SnapshotDate == currentSnapshotDate), "Expected Presumed-InScope for current year");
            Assert.That(org.OrganisationScopes.Count == 2 && org.OrganisationScopes.Any(os => os.Status == ScopeRowStatuses.Active && os.ScopeStatus == ScopeStatuses.PresumedOutOfScope && os.SnapshotDate == currentSnapshotDate.AddYears(-1)), "Expected Presumed-OutOfScope for previous year");

            Assert.That(org.OrganisationReferences == null || !org.OrganisationReferences.Any(), "There should be no organisation references saved");
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Any(), "No organisation SicCodes saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            //Check org
            Assert.That(org.Status == OrganisationStatuses.Pending, "Wrong org status");
            Assert.That(org.OrganisationName == stashedModel.GetManualEmployer().OrganisationName, "Organisation not saved");

            //Check org names
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == stashedModel.GetManualEmployer().OrganisationName && n.Source == stashedModel.GetManualEmployer().NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == stashedModel.GetManualEmployer().CompanyNumber, "Wrong company number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.CharityNumber))?.ReferenceValue == null, "Wrong charity number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.MutualNumber))?.ReferenceValue == null, "Wrong mutual number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == stashedModel.OtherName)?.ReferenceValue == null, "Wrong other reference name");

            Assert.That(org.CompanyNumber == stashedModel.GetManualEmployer().CompanyNumber, "Wrong company number");
            Assert.That(org.CompanyNumber == stashedModel.GetManualEmployer().CompanyNumber, "Wrong company number");

            //Check the address
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == employer.GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Pending, "Wrong address status");
            Assert.That(address.Source == stashedModel.GetManualEmployer().AddressSource, "Wrong address source");
            Assert.IsNull(org.GetAddress(), "Wrong latest address");

            //Check contact info
            Assert.IsTrue(!Text.IsAllNullOrWhiteSpace(user.ContactEmailAddress, user.ContactFirstName, user.ContactLastName, user.ContactPhoneNumber), "Contact info must be empty");

            //Check sic codes
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString() == stashedModel.GetManualEmployer().SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.GetManualEmployer().SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs[0].PINConfirmedDate, "Wrong PIN confirmed date");
            Assert.That(userOrgs[0].Address.GetAddressString() == address.GetAddressString(), "Wrong address for user org");

            //Check the organisation does not appear in search index
            var index= await Global.SearchRepository.GetAsync(org.OrganisationId.ToString(), nameof(EmployerSearchModel.OrganisationId));
            Assert.IsNull(index);
        }

        [Test]
        [Description("Ensure the ConfirmOrganisation saves existing private sector org")]
        public async Task RegisterController_ConfirmOrganisation_POST_ExistingPrivateSector_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
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
                PostCode = "W1 5qr",
            };
            var addressStatus = new AddressStatus { Address = address0, Status = AddressStatuses.Active };
            address0.AddressStatuses.Add(addressStatus);
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId="A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation()
            {
                OrganisationId = 1,
                OrganisationName = "Company1",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                CompanyNumber = "12345678",
                OrganisationAddresses = new List<OrganisationAddress> { address0 }
            };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic2);

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org0, address0, name, sic1, sic2);

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            var employer = org0.ToEmployerRecord();
            employer.ActiveAddressId = address0.AddressId;
            controller.PrivateSectorRepository.Insert(employer);

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                SectorType = SectorTypes.Private,

                AddressSource = null,
                Address1 = null,
                Address2 = null,
                Address3 = null,
                City = null,
                County = null,
                Country = null,
                PoBox = null,
                Postcode = null,
                IsUkAddress = true,

                ContactEmailAddress = null,
                ContactFirstName = null,
                ContactLastName = null,
                ContactJobTitle = null,
                ContactPhoneNumber = null,

                SicCodeIds = employer.SicCodeIds,
                SicCodes = sicCodes.Select(s => s.SicCodeId).ToList(),
                SicSource = "CoHo",

                SearchText = null,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = false,
                PINExpired = false,
                PINSent = false,
                Employers = null,
                SelectedEmployerIndex = -1,
                ManualEmployers = new List<EmployerRecord>() { employer },
                ManualEmployerIndex = 0,
                AddressReturnAction = null,
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == employer.OrganisationName);
            var userOrgs = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration))
            {
                Assert.That(result.ActionName == nameof(RegisterController.RequestReceived), "Redirected to the wrong view");
            }
            else
            {
                Assert.That(result.ActionName == nameof(RegisterController.PINSent), "Redirected to the wrong view");
            }
            

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            Assert.NotNull(org, "No organisation saved");
            Assert.That(userOrgs != null && userOrgs.Any(), "No user organisation saved");
            Assert.That(addresses != null && addresses.Any(), "No organisation address saved");
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Any(), "No organisation names saved");
            Assert.That(org.OrganisationReferences == null || !org.OrganisationReferences.Any(), "There should be no organisation references saved");
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Any(), "No organisation SicCodes saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            //Check org
            Assert.That(org.Status == OrganisationStatuses.Active, "Wrong org status");
            Assert.That(org.OrganisationName == stashedModel.GetManualEmployer().OrganisationName, "Organisation not saved");

            //Check org names
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == stashedModel.GetManualEmployer().OrganisationName && n.Source == stashedModel.GetManualEmployer().NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == stashedModel.GetManualEmployer().CompanyNumber, "Wrong company number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.CharityNumber))?.ReferenceValue == null, "Wrong charity number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.MutualNumber))?.ReferenceValue == null, "Wrong mutual number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == stashedModel.OtherName)?.ReferenceValue == null, "Wrong other reference name");

            Assert.That(org.CompanyNumber == stashedModel.GetManualEmployer().CompanyNumber, "Wrong company number");
            Assert.That(org.CompanyNumber == stashedModel.GetManualEmployer().CompanyNumber, "Wrong company number");

            //Check the address
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == unstashedModel.GetManualEmployer().GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Active, "Wrong address status");
            Assert.That(address.Source == stashedModel.GetManualEmployer().AddressSource, "Wrong address source");
            Assert.IsNotNull(org.GetAddress(), "Wrong latest address");

            //Check contact info
            Assert.IsTrue(!Text.IsAllNullOrWhiteSpace(user.ContactEmailAddress, user.ContactFirstName, user.ContactLastName, user.ContactPhoneNumber), "Contact info must be empty");

            //Check sic codes
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString(", ") == stashedModel.GetManualEmployer().SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.GetManualEmployer().SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs[0].PINConfirmedDate, "Wrong PIN confirmed date");
            Assert.That(userOrgs[0].Address.GetAddressString() == address.GetAddressString(), "Wrong address for user org");

            //Check the organisation exists in search
            var actualIndex = await controller.SearchBusinessLogic.SearchRepository.GetAsync(org.OrganisationId.ToString());
            var expectedIndex = org.ToEmployerSearchResult();
            expectedIndex.Compare(actualIndex);
        }

        [Test]
        [Description("Ensure the ConfirmOrganisation saves existing public sector org")]
        public async Task RegisterController_ConfirmOrganisation_POST_ExistingPublicSector_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
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
                PostCode = "W1 5qr",
            };
            var addressStatus = new AddressStatus { Address = address0, Status = AddressStatuses.Active };
            address0.AddressStatuses.Add(addressStatus);
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation()
            {
                OrganisationId = 1,
                OrganisationName = "Company1",
                SectorType = SectorTypes.Public,
                Status = OrganisationStatuses.Active,
                CompanyNumber = "12345678",
                OrganisationAddresses = new List<OrganisationAddress> { address0 }
            };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic2);

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org0, address0, name, sic1, sic2);

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            var employer = org0.ToEmployerRecord();
            employer.ActiveAddressId = address0.AddressId;
            controller.PrivateSectorRepository.Insert(employer);

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                SectorType = SectorTypes.Private,

                AddressSource = null,
                Address1 = null,
                Address2 = null,
                Address3 = null,
                City = null,
                County = null,
                Country = null,
                PoBox = null,
                Postcode = null,

                ContactEmailAddress = null,
                ContactFirstName = null,
                ContactLastName = null,
                ContactJobTitle = null,
                ContactPhoneNumber = null,

                SicCodeIds = employer.SicCodeIds,
                SicCodes = sicCodes.Select(s => s.SicCodeId).ToList(),
                SicSource = "CoHo",

                SearchText = null,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = false,
                PINExpired = false,
                PINSent = false,
                Employers = null,
                SelectedEmployerIndex = -1,
                ManualEmployers = new List<EmployerRecord>() { employer },
                ManualEmployerIndex = 0,
                AddressReturnAction = null,
                ConfirmReturnAction = nameof(RegisterController.ChooseOrganisation),
            };
            controller.StashModel(stashedModel);

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == employer.OrganisationName);
            var userOrgs = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.ServiceActivated), "Redirected to the wrong view");

            Assert.NotNull(org, "No organisation saved");
            Assert.That(userOrgs != null && userOrgs.Any(), "No user organisation saved");
            Assert.That(addresses != null && addresses.Any(), "No organisation address saved");
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Any(), "No organisation names saved");
            Assert.That(org.OrganisationReferences == null || !org.OrganisationReferences.Any(), "There should be no organisation references saved");
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Any(), "No organisation SicCodes saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            //Check org
            Assert.That(org.Status == OrganisationStatuses.Active, "Wrong org status");
            Assert.That(org.OrganisationName == stashedModel.GetManualEmployer().OrganisationName, "Organisation not saved");

            //Check org names
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == stashedModel.GetManualEmployer().OrganisationName && n.Source == stashedModel.GetManualEmployer().NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == stashedModel.GetManualEmployer().CompanyNumber, "Wrong company number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.CharityNumber))?.ReferenceValue == null, "Wrong charity number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.MutualNumber))?.ReferenceValue == null, "Wrong mutual number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == stashedModel.OtherName)?.ReferenceValue == null, "Wrong other reference name");

            Assert.That(org.CompanyNumber == stashedModel.GetManualEmployer().CompanyNumber, "Wrong company number");
            Assert.That(org.CompanyNumber == stashedModel.GetManualEmployer().CompanyNumber, "Wrong company number");

            //Check the address
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == stashedModel.GetManualEmployer().GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Active, "Wrong address status");
            Assert.That(address.Source == stashedModel.GetManualEmployer().AddressSource, "Wrong address source");
            Assert.IsNotNull(org.GetAddress(), "Wrong latest address");

            //Check contact info
            Assert.IsTrue(!Text.IsAllNullOrWhiteSpace(user.ContactEmailAddress, user.ContactFirstName, user.ContactLastName, user.ContactPhoneNumber), "Contact info must be empty");

            //Check sic codes
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString(", ") == stashedModel.GetManualEmployer().SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.GetManualEmployer().SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs[0].PINConfirmedDate, "Wrong PIN confirmed date");
            Assert.That(userOrgs[0].Address.GetAddressString() == address.GetAddressString(), "Wrong address for user org");

            //Check the organisation exists in search
            var actualIndex = await controller.SearchBusinessLogic.SearchRepository.GetAsync(org.OrganisationId.ToString());
            var expectedIndex = org.ToEmployerSearchResult();
            expectedIndex.Compare(actualIndex);
        }

        [Test]
        [Description("Ensure the ConfirmOrganisation saves existing private sector org with new address")]
        public async Task RegisterController_ConfirmOrganisation_POST_ExistingPrivateSectorNewAddress_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
                //AddressId = 1,
                Status = AddressStatuses.Active,
                Source = "CoHo",
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                TownCity = "City1",
                County = "County1",
                Country = "United Kingdom",
                PoBox = "PoBox 12",
                PostCode = "W1 5qr",
            };
            var addressStatus = new AddressStatus { Address = address0, Status = AddressStatuses.Active };
            address0.AddressStatuses.Add(addressStatus);
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation()
            {
                OrganisationId = 1,
                OrganisationName = "Company1",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                CompanyNumber = "12345678",
                OrganisationAddresses = new List<OrganisationAddress> { address0 }
            };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic2);

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org0, address0, name, sic1, sic2);

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            var employer = org0.ToEmployerRecord();
            employer.ActiveAddressId = address0.AddressId;
            controller.PrivateSectorRepository.Insert(employer);

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                SectorType = SectorTypes.Private,

                AddressSource = user.EmailAddress,
                Address1 = "New address1",
                Address2 = "New Address2",
                Address3 = "New Address 3",
                City = "New City",
                County = "New County",
                Country = "New Country",
                PoBox = "New Pobox",
                Postcode = "New PostCode",

                ContactEmailAddress = "test1@EmailAddress.com",
                ContactFirstName = "Contactfirst",
                ContactLastName = "Lastname",
                ContactJobTitle = "Boss",
                ContactPhoneNumber = "012345678",

                SicCodeIds = employer.SicCodeIds,
                SicCodes = sicCodes.Select(s => s.SicCodeId).ToList(),
                SicSource = "CoHo",

                SearchText = null,
                ManualAddress = true,
                SelectedAuthorised = false,
                ManualRegistration = false,
                PINExpired = false,
                PINSent = false,
                Employers = new PagedResult<EmployerRecord>() {
                    Results = new List<EmployerRecord> { employer }
                },
                SelectedEmployerIndex = 0,
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.AddContact),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == employer.OrganisationName);
            var userOrgs = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.RequestReceived), "Redirected to the wrong view");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            Assert.NotNull(org, "No organisation saved");
            Assert.That(userOrgs != null && userOrgs.Any(), "No user organisation saved");
            Assert.That(addresses != null && addresses.Count() == 2, "Organisation address saved");
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Any(), "No organisation names saved");
            Assert.That(org.OrganisationReferences == null || !org.OrganisationReferences.Any(), "There should be no organisation references saved");
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Any(), "No organisation SicCodes saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            //Check org
            Assert.That(org.Status == OrganisationStatuses.Active, "Wrong org status");
            Assert.That(org.OrganisationName == stashedModel.GetSelectedEmployer().OrganisationName, "Organisation not saved");

            //Check org names
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == stashedModel.GetSelectedEmployer().OrganisationName && n.Source == stashedModel.GetSelectedEmployer().NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.CharityNumber))?.ReferenceValue == null, "Wrong charity number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.MutualNumber))?.ReferenceValue == null, "Wrong mutual number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == stashedModel.OtherName)?.ReferenceValue == null, "Wrong other reference name");

            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");
            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");

            //Check the address
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == unstashedModel.GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Pending, "Wrong address status");
            Assert.That(address.Source == stashedModel.AddressSource, "Wrong address source");

            //Check contact info
            Assert.That(user.ContactEmailAddress == stashedModel.ContactEmailAddress, "Wrong contact email");
            Assert.That(user.ContactFirstName == stashedModel.ContactFirstName, "Wrong contact first name");
            Assert.That(user.ContactLastName == stashedModel.ContactLastName, "Wrong contact last name");
            Assert.That(user.ContactJobTitle == stashedModel.ContactJobTitle, "Wrong contact jobtitle");
            Assert.That(user.ContactPhoneNumber == stashedModel.ContactPhoneNumber, "Wrong contact phone");

            //Check sic codes
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString(", ") == stashedModel.GetSelectedEmployer().SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.GetSelectedEmployer().SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs[0].PINConfirmedDate, "Wrong PIN confirmed date");
            Assert.That(userOrgs[0].Address.GetAddressString() == address.GetAddressString(), "Wrong address for user org");

            //Check the organisation exists in search
            var actualIndex = await controller.SearchBusinessLogic.SearchRepository.GetAsync(org.OrganisationId.ToString());
            var expectedIndex = org.ToEmployerSearchResult();
            expectedIndex.Compare(actualIndex);
        }

        [Test]
        [Description("Ensure the ConfirmOrganisation saves existing public sector org with new address as pending when not authorised by email domain")]
        public async Task RegisterController_ConfirmOrganisation_POST_ExistingPublicSectorNewAddressNotAuthorised_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
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
                PostCode = "W1 5qr",
            };
            var addressStatus = new AddressStatus { Address = address0, Status = AddressStatuses.Active };
            address0.AddressStatuses.Add(addressStatus);
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation()
            {
                OrganisationId = 1,
                OrganisationName = "Company1",
                SectorType = SectorTypes.Public,
                Status = OrganisationStatuses.Active,
                CompanyNumber = "12345678",
                OrganisationAddresses = new List<OrganisationAddress> { address0 }
            };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic2);

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org0, address0, name, sic1, sic2);

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            var employer = org0.ToEmployerRecord();
            employer.ActiveAddressId = address0.AddressId;
            controller.PrivateSectorRepository.Insert(employer);

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                SectorType = SectorTypes.Private,

                AddressSource = user.EmailAddress,
                Address1 = "New address1",
                Address2 = "New Address2",
                Address3 = "New Address 3",
                City = "New City",
                County = "New County",
                Country = "New Country",
                PoBox = "New Pobox",
                Postcode = "New PostCode",

                ContactEmailAddress = "test1@EmailAddress.com",
                ContactFirstName = "Contactfirst",
                ContactLastName = "Lastname",
                ContactJobTitle = "Boss",
                ContactPhoneNumber = "012345678",

                SicCodeIds = employer.SicCodeIds,
                SicCodes = sicCodes.Select(s => s.SicCodeId).ToList(),
                SicSource = "CoHo",

                SearchText = null,
                ManualAddress = true,
                SelectedAuthorised = false,
                ManualRegistration = false,
                PINExpired = false,
                PINSent = false,
                Employers = new PagedResult<EmployerRecord>() {
                    Results = new List<EmployerRecord> { employer }
                },
                SelectedEmployerIndex = 0,
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.AddContact),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == employer.OrganisationName);
            var userOrgs = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.RequestReceived), "Redirected to the wrong view");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            Assert.NotNull(org, "No organisation saved");
            Assert.That(userOrgs != null && userOrgs.Any(), "No user organisation saved");
            Assert.That(addresses != null && addresses.Count() == 2, "Organisation address saved");
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Any(), "No organisation names saved");
            Assert.That(org.OrganisationReferences == null || !org.OrganisationReferences.Any(), "There should be no organisation references saved");
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Any(), "No organisation SicCodes saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            //Check org
            Assert.That(org.Status == OrganisationStatuses.Active, "Wrong org status");
            Assert.That(org.OrganisationName == stashedModel.GetSelectedEmployer().OrganisationName, "Organisation not saved");

            //Check org names
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == stashedModel.GetSelectedEmployer().OrganisationName && n.Source == stashedModel.GetSelectedEmployer().NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.CharityNumber))?.ReferenceValue == null, "Wrong charity number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.MutualNumber))?.ReferenceValue == null, "Wrong mutual number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == stashedModel.OtherName)?.ReferenceValue == null, "Wrong other reference name");

            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");
            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");

            //Check the address
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == unstashedModel.GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Pending, "Wrong address status");
            Assert.That(address.Source == stashedModel.AddressSource, "Wrong address source");

            //Check contact info
            Assert.That(user.ContactEmailAddress == stashedModel.ContactEmailAddress, "Wrong contact email");
            Assert.That(user.ContactFirstName == stashedModel.ContactFirstName, "Wrong contact first name");
            Assert.That(user.ContactLastName == stashedModel.ContactLastName, "Wrong contact last name");
            Assert.That(user.ContactJobTitle == stashedModel.ContactJobTitle, "Wrong contact jobtitle");
            Assert.That(user.ContactPhoneNumber == stashedModel.ContactPhoneNumber, "Wrong contact phone");

            //Check sic codes
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString(", ") == stashedModel.GetSelectedEmployer().SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.GetSelectedEmployer().SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs[0].PINConfirmedDate, "Wrong PIN confirmed date");
            Assert.That(userOrgs[0].Address.GetAddressString() == address.GetAddressString(), "Wrong address for user org");

            //Check the organisation exists in search
            var actualIndex = await controller.SearchBusinessLogic.SearchRepository.GetAsync(org.OrganisationId.ToString());
            var expectedIndex = org.ToEmployerSearchResult();
            expectedIndex.Compare(actualIndex);
        }

        [Test]
        [Description("Ensure the ConfirmOrganisation saves existing public sector org with new address as pending when authorised by email domain and existing address")]
        public async Task RegisterController_ConfirmOrganisation_POST_ExistingPublicSectorNewAddressAuthorised_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
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
                PostCode = "W1 5qr",
            };
            var addressStatus = new AddressStatus { Address = address0, Status = AddressStatuses.Active };
            address0.AddressStatuses.Add(addressStatus);
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation()
            {
                OrganisationId = 1,
                OrganisationName = "Company1",
                SectorType = SectorTypes.Public,
                Status = OrganisationStatuses.Active,
                CompanyNumber = "12345678",
                OrganisationAddresses = new List<OrganisationAddress> { address0 }
            };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic2);

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org0, address0, name, sic1, sic2);

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            var employer = org0.ToEmployerRecord();
            employer.ActiveAddressId = address0.AddressId;
            controller.PrivateSectorRepository.Insert(employer);

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                SectorType = SectorTypes.Private,

                AddressSource = user.EmailAddress,
                Address1 = "New address1",
                Address2 = "New Address2",
                Address3 = "New Address 3",
                City = "New City",
                County = "New County",
                Country = "New Country",
                PoBox = "New Pobox",
                Postcode = "New PostCode",

                ContactEmailAddress = "test1@EmailAddress.com",
                ContactFirstName = "Contactfirst",
                ContactLastName = "Lastname",
                ContactJobTitle = "Boss",
                ContactPhoneNumber = "012345678",

                SicCodeIds = employer.SicCodeIds,
                SicCodes = sicCodes.Select(s => s.SicCodeId).ToList(),
                SicSource = "CoHo",

                SearchText = null,
                ManualAddress = true,
                SelectedAuthorised = true,
                ManualRegistration = false,
                PINExpired = false,
                PINSent = false,
                Employers = new PagedResult<EmployerRecord>() {
                    Results = new List<EmployerRecord> { employer }
                },
                SelectedEmployerIndex = 0,
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.AddContact),
            };
            controller.StashModel(stashedModel);

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;

            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == employer.OrganisationName);
            var userOrgs = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.RequestReceived), "Redirected to the wrong view");

            Assert.NotNull(org, "No organisation saved");
            Assert.That(userOrgs != null && userOrgs.Any(), "No user organisation saved");
            Assert.That(addresses != null && addresses.Count() == 2, "Organisation address saved");
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Any(), "No organisation names saved");
            Assert.That(org.OrganisationReferences == null || !org.OrganisationReferences.Any(), "There should be no organisation references saved");
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Any(), "No organisation SicCodes saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            //Check org
            Assert.That(org.Status == OrganisationStatuses.Active, "Wrong org status");
            Assert.That(org.OrganisationName == stashedModel.GetSelectedEmployer().OrganisationName, "Organisation not saved");

            //Check org names
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == stashedModel.GetSelectedEmployer().OrganisationName && n.Source == stashedModel.GetSelectedEmployer().NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.CharityNumber))?.ReferenceValue == null, "Wrong charity number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.MutualNumber))?.ReferenceValue == null, "Wrong mutual number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == stashedModel.OtherName)?.ReferenceValue == null, "Wrong other reference name");

            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");
            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");

            //Check the address
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == stashedModel.GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Pending, "Wrong address status");
            Assert.That(address.Source == stashedModel.AddressSource, "Wrong address source");

            //Check contact info
            Assert.That(user.ContactEmailAddress == stashedModel.ContactEmailAddress, "Wrong contact email");
            Assert.That(user.ContactFirstName == stashedModel.ContactFirstName, "Wrong contact first name");
            Assert.That(user.ContactLastName == stashedModel.ContactLastName, "Wrong contact last name");
            Assert.That(user.ContactJobTitle == stashedModel.ContactJobTitle, "Wrong contact jobtitle");
            Assert.That(user.ContactPhoneNumber == stashedModel.ContactPhoneNumber, "Wrong contact phone");

            //Check sic codes
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString(", ") == stashedModel.GetSelectedEmployer().SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.GetSelectedEmployer().SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs[0].PINConfirmedDate, "Wrong PIN confirmed date");
            Assert.That(userOrgs[0].Address.GetAddressString() == address.GetAddressString(), "Wrong address for user org");

            //Check the organisation exists in search
            var actualIndex = await controller.SearchBusinessLogic.SearchRepository.GetAsync(org.OrganisationId.ToString());
            var expectedIndex = org.ToEmployerSearchResult();
            expectedIndex.Compare(actualIndex);
        }

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("Ensure the ConfirmOrganisation saves existing public sector org with first address as active when authorised by email domain and no address")]
        public async Task RegisterController_ConfirmOrganisation_POST_ExistingPublicSectorFirstAddressAuthorised_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
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
                PostCode = "W1 5qr",
                IsUkAddress = true
            };
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation() { OrganisationId = 1, OrganisationName = "Company1", SectorType = SectorTypes.Public, Status = OrganisationStatuses.Pending, CompanyNumber = "12345678" };

            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic2);

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org0, address0, name, sic1, sic2);

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            var employer = org0.ToEmployerRecord();
            employer.ActiveAddressId = address0.AddressId;
            controller.PrivateSectorRepository.Insert(employer);

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "Acme ltd",
                NameSource = "CoHo",
                CompanyNumber = "12345678",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                SectorType = SectorTypes.Private,

                AddressSource = user.EmailAddress,
                Address1 = "New address1",
                Address2 = "New Address2",
                Address3 = "New Address 3",
                City = "New City",
                County = "New County",
                Country = "New Country",
                PoBox = "New Pobox",
                Postcode = "New PostCode",
                IsUkAddress = true,

                ContactEmailAddress = "test1@EmailAddress.com",
                ContactFirstName = "Contactfirst",
                ContactLastName = "Lastname",
                ContactJobTitle = "Boss",
                ContactPhoneNumber = "012345678",

                SicCodeIds = employer.SicCodeIds,
                SicCodes = sicCodes.Select(s => s.SicCodeId).ToList(),
                SicSource = "CoHo",

                SearchText = null,
                ManualAddress = true,
                SelectedAuthorised = true,
                ManualRegistration = false,
                PINExpired = false,
                PINSent = false,
                Employers = new PagedResult<EmployerRecord>() {
                    Results = new List<EmployerRecord> { employer }
                },
                SelectedEmployerIndex = 0,
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.AddContact),
            };
            controller.StashModel(stashedModel);

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;

            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == employer.OrganisationName);
            var userOrgs = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.ServiceActivated), "Redirected to the wrong view");

            Assert.NotNull(org, "No organisation saved");
            Assert.That(userOrgs != null && userOrgs.Any(), "No user organisation saved");
            Assert.That(addresses != null && addresses.Count() == 1, "Organisation address saved");
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Any(), "No organisation names saved");
            Assert.That(org.OrganisationReferences == null || !org.OrganisationReferences.Any(), "There should be no organisation references saved");
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Any(), "No organisation SicCodes saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            //Check org
            Assert.That(org.Status == OrganisationStatuses.Active, "Wrong org status");
            Assert.That(org.OrganisationName == stashedModel.GetSelectedEmployer().OrganisationName, "Organisation not saved");

            //Check org names
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == stashedModel.GetSelectedEmployer().OrganisationName && n.Source == stashedModel.GetSelectedEmployer().NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.CharityNumber))?.ReferenceValue == null, "Wrong charity number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.MutualNumber))?.ReferenceValue == null, "Wrong mutual number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == stashedModel.OtherName)?.ReferenceValue == null, "Wrong other reference name");

            Assert.That(org.CompanyNumber == stashedModel.GetSelectedEmployer().CompanyNumber, "Wrong company number");

            //Check the address
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == stashedModel.GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Active, "Wrong address status");
            Assert.That(address.Source == stashedModel.AddressSource, "Wrong address source");
            Assert.IsNotNull(org.GetAddress(), "Wrong latest address");
            Assert.That(org.GetAddress().AddressId == address.AddressId, "Wrong latest address");
            Assert.That(org.Status == OrganisationStatuses.Active, "Wrong organisation status");

            //Check contact info
            Assert.IsNull(user.ContactEmailAddress, "Wrong contact email");
            Assert.IsNull(user.ContactFirstName, "Wrong contact first name");
            Assert.IsNull(user.ContactLastName, "Wrong contact last name");
            Assert.IsNull(user.ContactJobTitle, "Wrong contact jobtitle");
            Assert.IsNull(user.ContactPhoneNumber, "Wrong contact phone");

            //Check sic codes
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString(", ") == stashedModel.GetSelectedEmployer().SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.GetSelectedEmployer().SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNotNull(userOrgs[0].PINConfirmedDate, "Wrong PIN confirmed date");
            Assert.That(userOrgs[0].Address.GetAddressString() == address.GetAddressString(), "Wrong address for user org");

            //Check the organisation exists in search
            var actualIndex = await controller.SearchBusinessLogic.SearchRepository.GetAsync(org.OrganisationId.ToString());
            var expectedIndex = org.ToEmployerSearchResult();
            expectedIndex.Compare(actualIndex);
        }
        [Test]
        [Description("Ensure the ConfirmOrganisation saves new manually registered private sector org")]
        public async Task RegisterController_ConfirmOrganisation_POST_ManualPrivateSector_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
                Status = AddressStatuses.Active,
                Source = "CoHo",
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                TownCity = "City1",
                County = "County1",
                Country = "United Kingdom",
                PoBox = "PoBox 12",
                PostCode = "W1 5qr",
            };
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var sicCode3 = new SicCode() { SicCodeId = 10910, Description = "Desc3", SicSection = new SicSection() { SicSectionId = "C", Description = "Section3" } };
            var sicCode4 = new SicCode() { SicCodeId = 11060, Description = "Desc4", SicSection = new SicSection() { SicSectionId = "D", Description = "Section4" } };
            var org0 = new Organisation()
            {
                OrganisationName = "Company1",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                CompanyNumber = "12345678",
                OrganisationAddresses = new List<OrganisationAddress> { address0 }
            };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic2);

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org0, address0, name, sic1, sic2, sicCode3, sicCode4);

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            var employer = org0.ToEmployerRecord();
            employer.ActiveAddressId = address0.AddressId;
            controller.PrivateSectorRepository.Insert(employer);

            var currentSnapshotDate = employer.SectorType.GetAccountingStartDate();

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "New ACME Ltd",
                NameSource = "CoHo",
                CompanyNumber = "11234567",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                SectorType = SectorTypes.Private,

                AddressSource = user.EmailAddress,
                Address1 = "New address1",
                Address2 = "New Address2",
                Address3 = "New Address 3",
                City = "New City",
                County = "New County",
                Country = "New Country",
                PoBox = "New Pobox",
                Postcode = "New PostCode",

                ContactEmailAddress = "test1@EmailAddress.com",
                ContactFirstName = "Contactfirst",
                ContactLastName = "Lastname",
                ContactJobTitle = "Boss",
                ContactPhoneNumber = "012345678",

                SicCodeIds = "10910, 11060",
                SicCodes = new List<int>() { sicCode3.SicCodeId, sicCode4.SicCodeId },
                SicSource = user.EmailAddress,

                SearchText = null,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = true,
                PINExpired = false,
                PINSent = false,
                Employers = null,
                SelectedEmployerIndex = -1,
                ManualEmployers = new List<EmployerRecord>() { employer },
                ManualEmployerIndex = 1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.AddContact),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == stashedModel.OrganisationName);
            var userOrgs = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.RequestReceived), "Redirected to the wrong view");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            Assert.NotNull(org, "No organisation saved");
            Assert.That(userOrgs != null && userOrgs.Any(), "No user organisation saved");
            Assert.That(addresses != null && addresses.Count() == 1, "Organisation address saved");
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Any(), "No organisation names saved");
            Assert.That(org.OrganisationReferences != null && org.OrganisationReferences.Count() == 3, "There should be 3 organisation references saved");
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Count() == 2, "2 organisation SicCodes not saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            //Check org
            Assert.That(org.Status == OrganisationStatuses.Pending, "Wrong org status");
            Assert.That(org.OrganisationName == stashedModel.OrganisationName, "Organisation not saved");
            Assert.That(!string.IsNullOrWhiteSpace(org.EmployerReference), "Expected new EmployerReferenceto be  assigned");
            Assert.That(org.OrganisationScopes.Count == 2 && org.OrganisationScopes.Any(os => os.Status == ScopeRowStatuses.Active && os.ScopeStatus == ScopeStatuses.PresumedInScope && os.SnapshotDate == currentSnapshotDate), "Expected Presumed-InScope for current year");
            Assert.That(org.OrganisationScopes.Count == 2 && org.OrganisationScopes.Any(os => os.Status == ScopeRowStatuses.Active && os.ScopeStatus == ScopeStatuses.PresumedOutOfScope && os.SnapshotDate == currentSnapshotDate.AddYears(-1)), "Expected Presumed-OutOfScope for previous year");

            //Check org names
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == stashedModel.OrganisationName && n.Source == stashedModel.NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == stashedModel.CompanyNumber, "Wrong company number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.CharityNumber))?.ReferenceValue == stashedModel.CharityNumber, "Wrong charity number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.MutualNumber))?.ReferenceValue == stashedModel.MutualNumber, "Wrong mutual number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == stashedModel.OtherName)?.ReferenceValue == stashedModel.OtherValue, "Wrong other reference name");

            Assert.That(org.CompanyNumber == stashedModel.CompanyNumber, "Wrong company number");

            //Check the address
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == unstashedModel.GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Pending, "Wrong address status");
            Assert.That(address.Source == stashedModel.AddressSource, "Wrong address source");
            Assert.IsNull(org.GetAddress(), "Wrong latest address");

            //Check contact info
            Assert.That(user.ContactEmailAddress == stashedModel.ContactEmailAddress, "Wrong contact email");
            Assert.That(user.ContactFirstName == stashedModel.ContactFirstName, "Wrong contact first name");
            Assert.That(user.ContactLastName == stashedModel.ContactLastName, "Wrong contact last name");
            Assert.That(user.ContactJobTitle == stashedModel.ContactJobTitle, "Wrong contact jobtitle");
            Assert.That(user.ContactPhoneNumber == stashedModel.ContactPhoneNumber, "Wrong contact phone");

            //Check sic codes
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString(", ") == stashedModel.SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs[0].PINConfirmedDate, "Wrong PIN confirmed date");

            //Check the organisation does not exists in search
            var actualIndex = await Global.SearchRepository.GetAsync(org.OrganisationId.ToString(),nameof(EmployerSearchModel.OrganisationId));
            Assert.IsNull(actualIndex,"Organisation should not exist on search");
        }

        [Test]
        [Description("Ensure the ConfirmOrganisation saves new manually registered public sector org")]
        public async Task RegisterController_ConfirmOrganisation_POST_ManualPublicSector_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User() { UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
                Status = AddressStatuses.Active,
                Source = "CoHo",
                Address1 = "123",
                Address2 = "evergreen terrace",
                Address3 = "Westminster",
                TownCity = "City1",
                County = "County1",
                Country = "United Kingdom",
                PoBox = "PoBox 12",
                PostCode = "W1 5qr",
            };
            var sicCode1a = new SicCode() { SicCodeId = 1, Description = "Public sector", SicSection = new SicSection() { SicSectionId = "A", Description = "Public sector" } };
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSection = new SicSection() { SicSectionId = "B", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSection = new SicSection() { SicSectionId = "C", Description = "Section2" } };
            var sicCode3 = new SicCode() { SicCodeId = 10910, Description = "Desc3", SicSection = new SicSection() { SicSectionId = "D", Description = "Section3" } };
            var sicCode4 = new SicCode() { SicCodeId = 11060, Description = "Desc4", SicSection = new SicSection() { SicSectionId = "E", Description = "Section4" } };
            var org0 = new Organisation()
            {
                OrganisationName = "Company1",
                SectorType = SectorTypes.Public,
                Status = OrganisationStatuses.Active,
                CompanyNumber = "12345678",
                OrganisationAddresses = new List<OrganisationAddress> { address0 }
            };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId };
            org0.OrganisationSicCodes.Add(sic2);

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org0, address0, name, sic1, sic2, sicCode3, sicCode4, sicCode1a);

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            var employer = org0.ToEmployerRecord();
            employer.ActiveAddressId = address0.AddressId;
            controller.PrivateSectorRepository.Insert(employer);

            var currentSnapshotDate = employer.SectorType.GetAccountingStartDate();

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                OrganisationName = "New ACME Ltd",
                NameSource = "CoHo",
                CompanyNumber = "11234567",
                CharityNumber = "Charity1",
                MutualNumber = "Mutual1",
                OtherName = "OtherName1",
                OtherValue = "OtherValue1",
                DateOfCessation = VirtualDateTime.Now,
                SectorType = SectorTypes.Public,

                AddressSource = user.EmailAddress,
                Address1 = "New address1",
                Address2 = "New Address2",
                Address3 = "New Address 3",
                City = "New City",
                County = "New County",
                Country = "New Country",
                PoBox = "New Pobox",
                Postcode = "New PostCode",

                ContactEmailAddress = "test1@EmailAddress.com",
                ContactFirstName = "Contactfirst",
                ContactLastName = "Lastname",
                ContactJobTitle = "Boss",
                ContactPhoneNumber = "012345678",

                SicCodeIds = "1, 10910, 11060",
                SicCodes = new List<int>() { sicCode1a.SicCodeId, sicCode3.SicCodeId, sicCode4.SicCodeId },
                SicSource = user.EmailAddress,

                SearchText = null,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = true,
                PINExpired = false,
                PINSent = false,
                Employers = null,
                SelectedEmployerIndex = -1,
                ManualEmployers = new List<EmployerRecord>() { employer },
                ManualEmployerIndex = 1,
                AddressReturnAction = nameof(RegisterController.ConfirmOrganisation),
                ConfirmReturnAction = nameof(RegisterController.AddContact),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == stashedModel.OrganisationName);
            var userOrgs = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.That(result.ActionName.ToString() == nameof(RegisterController.RequestReceived), "Redirected to the wrong view");

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            Assert.NotNull(org, "No organisation saved");
            Assert.That(userOrgs != null && userOrgs.Any(), "No user organisation saved");
            Assert.That(addresses != null && addresses.Count() == 1, "Organisation address saved");
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Any(), "No organisation names saved");
            Assert.That(org.OrganisationReferences != null && org.OrganisationReferences.Count() == 3, "There should be 3 organisation references saved");
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Count() == 3, "3 organisation SicCodes not saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            //Check org
            Assert.That(org.Status == OrganisationStatuses.Pending, "Wrong org status");
            Assert.That(org.OrganisationName == stashedModel.OrganisationName, "Organisation not saved");
            Assert.That(!string.IsNullOrWhiteSpace(org.EmployerReference), "Expected new EmployerReferenceto be  assigned");
            Assert.That(org.OrganisationScopes.Count == 2 && org.OrganisationScopes.Any(os => os.Status == ScopeRowStatuses.Active && os.ScopeStatus == ScopeStatuses.PresumedInScope && os.SnapshotDate == currentSnapshotDate), "Expected Presumed-InScope for current year");
            Assert.That(org.OrganisationScopes.Count == 2 && org.OrganisationScopes.Any(os => os.Status == ScopeRowStatuses.Active && os.ScopeStatus == ScopeStatuses.PresumedOutOfScope && os.SnapshotDate == currentSnapshotDate.AddYears(-1)), "Expected Presumed-OutOfScope for previous year");

            //Check org names
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == stashedModel.OrganisationName && n.Source == stashedModel.NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == stashedModel.CompanyNumber, "Wrong company number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.CharityNumber))?.ReferenceValue == stashedModel.CharityNumber, "Wrong charity number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == nameof(OrganisationViewModel.MutualNumber))?.ReferenceValue == stashedModel.MutualNumber, "Wrong mutual number");
            Assert.That(references.FirstOrDefault(r => r.ReferenceName == stashedModel.OtherName)?.ReferenceValue == stashedModel.OtherValue, "Wrong other reference name");

            Assert.That(org.CompanyNumber == stashedModel.CompanyNumber, "Wrong company number");

            //Check the address
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == unstashedModel.GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Pending, "Wrong address status");
            Assert.That(address.Source == stashedModel.AddressSource, "Wrong address source");
            Assert.IsNull(org.GetAddress(), "Wrong latest address");

            //Check contact info
            Assert.That(user.ContactEmailAddress == stashedModel.ContactEmailAddress, "Wrong contact email");
            Assert.That(user.ContactFirstName == stashedModel.ContactFirstName, "Wrong contact first name");
            Assert.That(user.ContactLastName == stashedModel.ContactLastName, "Wrong contact last name");
            Assert.That(user.ContactJobTitle == stashedModel.ContactJobTitle, "Wrong contact jobtitle");
            Assert.That(user.ContactPhoneNumber == stashedModel.ContactPhoneNumber, "Wrong contact phone");

            //Check sic codes
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString(", ") == stashedModel.SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs[0].PINConfirmedDate, "Wrong PIN confirmed date");

            //Check the organisation does not exists in search
            var actualIndex = await Global.SearchRepository.GetAsync(org.OrganisationId.ToString(), nameof(EmployerSearchModel.OrganisationId));
            Assert.IsNull(actualIndex, "Organisation should not exist on search");
        }

        [Test]
        [Description("Ensure the ConfirmOrganisation saves pending private sector org with same address")]
        public async Task RegisterController_ConfirmOrganisation_POST_ExistingPrivatePendingSameAddress_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user1 = new User() { UserId = 1, EmailAddress = "test1@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var user2 = new User() { UserId = 2, EmailAddress = "test2@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
                AddressId = 1,
                Status = AddressStatuses.Pending,
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
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var org0 = new Organisation() { OrganisationId = 1, OrganisationName = "Company1", SectorType = SectorTypes.Private, Status = OrganisationStatuses.Pending, CompanyNumber = "12345678" };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { OrganisationNameId = 1, Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source="CoHo" };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic2);

            var userOrg = new UserOrganisation {
                Organisation=org0,
                User=user1,
                PINSentDate=VirtualDateTime.Now,
                Address=address0,
                Method=RegistrationMethods.PinInPost,
            };

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2
            };

            var controller = UiTestHelper.GetController<RegisterController>(user2.UserId, routeData, user1, user2, org0, address0, name, sic1, sic2, userOrg, sicCodes);

            var employer = org0.ToEmployerRecord();

            employer.ActiveAddressId = 0;
            employer.RegistrationStatus = null;
            employer.References = new Dictionary<string, string>();
            employer.Address1 = address0.Address1;
            employer.Address2 = address0.Address2;
            employer.Address3 = address0.Address3;
            employer.City = address0.TownCity;
            employer.County = address0.County;
            employer.Country = address0.Country;
            employer.PostCode = address0.PostCode;
            employer.PoBox = address0.PoBox;
            employer.AddressSource = "CoHo";
            employer.NameSource = "CoHo";
            employer.SicSource = "CoHo";

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                SectorType = SectorTypes.Private,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = false,
                IsUkAddress = true,
                PINExpired = false,
                PINSent = false,
                Employers = new PagedResult<EmployerRecord>() {
                    Results = new EmployerRecord[1] { employer }.ToList()
                },
                SicCodes=null,
                SelectedEmployerIndex = 0,
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                ConfirmReturnAction = nameof(RegisterController.ConfirmOrganisation),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.SicCodes = null;

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == employer.OrganisationName);
            var userOrgs1 = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user1.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var userOrgs2 = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user2.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration))
            {
                Assert.That(result.ActionName == nameof(RegisterController.RequestReceived), "Redirected to the wrong view");
            }
            else
            {
                Assert.That(result.ActionName == nameof(RegisterController.PINSent), "Redirected to the wrong view");
            }
            

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            //Check org
            Assert.NotNull(org, "No organisation saved");
            Assert.That(org.Status == OrganisationStatuses.Pending, "Organisation should still be pending");
            Assert.That(org.OrganisationName == stashedModel.GetSelectedEmployer().OrganisationName, "Organisation not saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            var selectedEmployer = stashedModel.GetSelectedEmployer();

            //Check org names
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Count == 1, "Only 1 organisation name should exist");
            Assert.That(org.OrganisationNames.First().Name == org.OrganisationName, "Organisation name doesnt match name history");
            Assert.That(org.OrganisationReferences == null || !org.OrganisationReferences.Any(), "There should be no organisation references saved");
            Assert.IsTrue(org.OrganisationNames.Any(n => n.Name == selectedEmployer.OrganisationName && n.Source == stashedModel.GetSelectedEmployer().NameSource), "Wrong name saved or name source");

            //Check references
            Assert.That(org.CompanyNumber == selectedEmployer.CompanyNumber, "Wrong company number");

            //Check the address
            Assert.That(addresses.Count == 1, "Organisation address not saved");
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == selectedEmployer.GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Pending, "Address should still be pending");
            Assert.That(address.Source == selectedEmployer.AddressSource, "Wrong address source");

            //Check sic codes
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Count == 2, "There should only be 2 SicCodes");
            Assert.That(sics.Select(s => s.SicCodeId).ToDelimitedString(", ") == selectedEmployer.SicCodeIds, "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == stashedModel.GetSelectedEmployer().SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs2.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs2[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs2[0].PINConfirmedDate, "Wrong PIN confirmed date");
            Assert.That(userOrgs2[0].Address.GetAddressString() == address.GetAddressString(), "Wrong address for user org");

            //Check the organisation does not exist in search
            var actualIndex = await controller.SearchBusinessLogic.SearchRepository.GetAsync(org.OrganisationId.ToString());
            Assert.IsNull(actualIndex, "Organisation should not exist in search index");
        }

        [Test]
        [Description("Ensure the ConfirmOrganisation saves pending private sector org with new name, address, SIC codes")]
        public async Task RegisterController_ConfirmOrganisation_POST_ExistingPrivatePendingNewNameAddressSic_Success()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user1 = new User() { UserId = 1, EmailAddress = "test1@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var user2 = new User() { UserId = 2, EmailAddress = "test2@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address0 = new OrganisationAddress() {
                Status = AddressStatuses.Pending,
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
            var sicCode1 = new SicCode() { SicCodeId = 2100, Description = "Desc1", SicSectionId = "A", SicSection = new SicSection() { SicSectionId = "A", Description = "Section1" } };
            var sicCode2 = new SicCode() { SicCodeId = 10520, Description = "Desc2", SicSectionId = "B", SicSection = new SicSection() { SicSectionId = "B", Description = "Section2" } };
            var sicCode3 = new SicCode() { SicCodeId = 5000, Description = "Desc3", SicSectionId = "C", SicSection = new SicSection() { SicSectionId = "C", Description = "Section3" } };
            var sicCode4 = new SicCode() { SicCodeId = 6000, Description = "Desc4", SicSectionId = "D", SicSection = new SicSection() { SicSectionId = "D", Description = "Section4" } };

            var org0 = new Organisation() { OrganisationId = 1, OrganisationName = "Company1", SectorType = SectorTypes.Private, Status = OrganisationStatuses.Pending, CompanyNumber = "12345678" };
            org0.OrganisationAddresses.Add(address0);
            var name = new OrganisationName() { Name = org0.OrganisationName, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationNames.Add(name);
            var sic1 = new OrganisationSicCode() { SicCode = sicCode1, SicCodeId = 2100, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic1);
            var sic2 = new OrganisationSicCode() { SicCode = sicCode2, SicCodeId = 10520, Created = org0.Created, Organisation = org0, OrganisationId = org0.OrganisationId, Source = "CoHo" };
            org0.OrganisationSicCodes.Add(sic2);

            var userOrg = new UserOrganisation {
                Organisation = org0,
                User = user1,
                PINSentDate = VirtualDateTime.Now,
                Address = address0,
                Method = RegistrationMethods.PinInPost,
            };

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org0.OrganisationScopes.Add(new OrganisationScope() {
                Organisation = org0,
                ScopeStatus = ScopeStatuses.InScope,
                SnapshotDate = org0.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                Status = ScopeRowStatuses.Active
            });

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ConfirmOrganisation));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user2.UserId, routeData, user1, user2, sicCode1, sicCode2, sicCode3, sicCode4, org0, address0, name, sic1, sic2, userOrg);

            var sicCodes = new List<SicCode>() {
                sicCode1,sicCode2,sicCode3,sicCode4
            };

            var employer = new EmployerRecord {
                OrganisationId = org0.OrganisationId,
                SectorType = org0.SectorType,
                OrganisationName = "New Org Name",
                CompanyNumber = org0.CompanyNumber,
                Address1 = "New Address1",
                Address2 = "New Address2",
                Address3 = "New Address3",
                City = "New City",
                County = "New County",
                Country = "New Country",
                PostCode = "New PostCode",
                SicCodeIds = "5000,6000",
                NameSource = "CoHo",
                AddressSource = "CoHo",
                SicSource = "CoHo"
            };

            //Set the initial model
            var stashedModel = new OrganisationViewModel() {
                NoReference = false,
                SectorType = SectorTypes.Private,
                ManualAddress = false,
                SelectedAuthorised = false,
                ManualRegistration = false,
                IsUkAddress = true,
                PINExpired = false,
                PINSent = false,
                Employers = new PagedResult<EmployerRecord>() {
                    Results = new EmployerRecord[1] { employer }.ToList()
                },
                SicCodes = null,
                SelectedEmployerIndex = 0,
                ManualEmployers = new List<EmployerRecord>(),
                ManualEmployerIndex = -1,
                ConfirmReturnAction = nameof(RegisterController.ConfirmOrganisation),
            };
            controller.StashModel(stashedModel);

            //Set the expected model
            var expectedModel = stashedModel.GetClone();
            expectedModel.SicCodes = null;

            //ACT:
            var result = await controller.ConfirmOrganisation(stashedModel, "confirm") as RedirectToActionResult;
            var unstashedModel = controller.UnstashModel<OrganisationViewModel>();
            var org = controller.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationName == employer.OrganisationName);
            var userOrgs1 = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user1.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var userOrgs2 = org == null ? null : controller.DataRepository.GetAll<UserOrganisation>().Where(uo => uo.UserId == user2.UserId && uo.OrganisationId == org.OrganisationId).ToList();
            var addresses = org == null ? null : controller.DataRepository.GetAll<OrganisationAddress>().Where(oa => oa.OrganisationId == org.OrganisationId).ToList();
            var references = org == null ? null : controller.DataRepository.GetAll<OrganisationReference>().Where(on => on.OrganisationId == org.OrganisationId).ToList();
            var sics = org == null ? null : controller.DataRepository.GetAll<OrganisationSicCode>().Where(os => os.OrganisationId == org.OrganisationId).ToList();

            //ASSERT:
            Assert.NotNull(result, "Expected RedirectToRouteResult");
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration))
            {
                Assert.That(result.ActionName == nameof(RegisterController.RequestReceived), "Redirected to the wrong view");
            }
            else
            {
                Assert.That(result.ActionName == nameof(RegisterController.PINSent), "Redirected to the wrong view");
            }

            //Check result is same as expected
            expectedModel.Compare(unstashedModel);

            //Check org
            Assert.NotNull(org, "No organisation saved");
            Assert.That(org.Status == OrganisationStatuses.Pending, "Organisation should still be pending");
            Assert.That(org.OrganisationName == employer.OrganisationName, "Organisation not saved");
            Assert.That(controller.ReportingOrganisationId == org.OrganisationId, "Reporting organisation not saved");

            var selectedEmployer = stashedModel.GetSelectedEmployer();

            //Check org names
            Assert.That(org.OrganisationNames != null && org.OrganisationNames.Count == 2, "Expected 2 organisation names");
            Assert.That(org.OrganisationNames.OrderByDescending(n=>n.Created).First().Name == org.OrganisationName, "Organisation name doesnt match name history");
            Assert.That(org.OrganisationReferences == null || !org.OrganisationReferences.Any(), "There should be no organisation references saved");
            Assert.That(org.OrganisationNames.OrderByDescending(n => n.Created).First().Source == selectedEmployer.NameSource, "Organisation name source doesnt match selected");

            //Check references
            Assert.That(org.CompanyNumber == selectedEmployer.CompanyNumber, "Wrong company number");

            //Check the address
            Assert.That(addresses.Count == 2, "Organisation address not saved");
            var address = addresses.FirstOrDefault(a => a.GetAddressString() == selectedEmployer.GetFullAddress());
            Assert.IsNotNull(address, "Address not saved");
            Assert.That(address.Status == AddressStatuses.Pending, "Address should still be pending");
            Assert.That(address.Source == selectedEmployer.AddressSource, "Wrong address source");

            //Check sic codes
            Assert.That(org.OrganisationSicCodes != null && org.OrganisationSicCodes.Count == 4, "There should now be 4 SicCodes");
            Assert.That(org.GetSicCodeIds().SequenceEqual(selectedEmployer.GetSicCodes()), "Wrong sic codes saved");
            Assert.That(org.GetSicSource() == selectedEmployer.SicSource, "Wrong sic source saved");

            //Check user org
            Assert.That(userOrgs2.Count == 1, "Wrong number of user orgs");
            Assert.IsNull(userOrgs2[0].PINSentDate, "Wrong PIN sent date");
            Assert.IsNull(userOrgs2[0].PINConfirmedDate, "Wrong PIN confirmed date");
            Assert.That(userOrgs2[0].Address.AddressId == address.AddressId, "Wrong address for user org");
            Assert.That(userOrgs2[0].Address.GetAddressString() == address.GetAddressString(), "Wrong address for user org");

            //Check the organisation does not exist in search
            var actualIndex = await controller.SearchBusinessLogic.SearchRepository.GetAsync(org.OrganisationId.ToString());
            Assert.IsNull(actualIndex, "Organisation should not exist in search index");
        }
        #endregion
    }
}
