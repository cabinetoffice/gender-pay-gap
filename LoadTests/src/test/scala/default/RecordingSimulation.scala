package default

import scala.concurrent.duration._
import scala.util.Random

import io.gatling.core.Predef._
import io.gatling.http.Predef._

class RecordingSimulation extends Simulation {

	val MAX_NUM_USERS = 20000
	// Pauses are uniform duration between these two:
	val PAUSE_MIN_DUR = 1 seconds
	val PAUSE_MAX_DUR = 10 seconds

	val CURRENT_YEAR = "2021-22"

	val httpProtocol = http
		.baseUrl("https://gender-pay-gap-loadtest.london.cloudapps.digital")
		.inferHtmlResources()
		.acceptHeader("*/*")
		.acceptEncodingHeader("gzip, deflate")
		.acceptLanguageHeader("en-GB,en;q=0.5")
		.userAgentHeader("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0")

	val headers_0 = Map(
		"Accept" -> "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
		"Upgrade-Insecure-Requests" -> "1")

	val headers_1 = Map("Accept" -> "image/webp,*/*")

	val headers_2 = Map("X-Requested-With" -> "XMLHttpRequest")

	val headers_3 = Map(
		"Accept" -> "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
		"Origin" -> "https://gender-pay-gap-loadtest.london.cloudapps.digital",
		"Upgrade-Insecure-Requests" -> "1")

	val headers_4 = Map(
		"Accept" -> "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
		"DNT" -> "1",
		"Pragma" -> "no-cache",
		"Upgrade-Insecure-Requests" -> "1")

	val headers_5 = Map(
		"DNT" -> "1",
		"Pragma" -> "no-cache")

	val searchFeeder = Iterator.continually(Map("searchCriterion" -> s"test_${Random.nextInt(MAX_NUM_USERS) + 1}"))
	val registrationFeeder = Iterator.continually(Map("email" -> (Random.alphanumeric.take(20).mkString + "@loadtest.com")))
	val usersOrganisationsFeeder = csv("users_organisations.csv").circular

	object HomePage {
		val visit = feed(usersOrganisationsFeeder)
			.exec(http("Visit home page")
			.get("/")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("Search and compare gender pay gap data"))
			.resources(http("Load logo")
				.get("/public/govuk_template/assets/stylesheets/images/gov.uk_logotype_crown.png")
				.headers(headers_1),
				http("Load search button")
					.get("/public/images/search-button.png")
					.headers(headers_1),
				http("Load licence image")
					.get("/public/govuk_template/assets/stylesheets/images/open-government-licence_2x.png")
					.headers(headers_1),
				http("Load crest")
					.get("/public/govuk_template/assets/stylesheets/images/govuk-crest-2x.png")
					.headers(headers_1)))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val search = feed(searchFeeder)
			.exec(http("Search for an organisation")
			.get("/viewing/search-results?t=1&search=${searchCriterion}")
			.headers(headers_2)
			.check(
				status.is(200),
				css("a[data-loadtestid='${searchCriterion}']", "href").find.saveAs("FirstSearchResultLink")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
			.exec(http("Select an organisation")
			.get("${FirstSearchResultLink}")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("Gender pay gap reports")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object SignInPage {
		val visit = exec(http("Visit sign in page")
			.get("/login")
			.headers(headers_0)
			.check(
				status.in(200),
				regex("Sign in"),
				regex("If you have a user account, enter your email address and password"),
				css("input[name='ReturnUrl'","value").saveAs("returnUrl"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"))
			.resources(http("Load important icon")
				.get("/public/images/icon-important-2x.png"),
				http("Load licence image")
					.get("/public/govuk_template/assets/stylesheets/images/open-government-licence_2x.png"),
				http("Load logo")
					.get("/public/govuk_template/assets/stylesheets/images/gov.uk_logotype_crown.png"),
				http("Load crest")
					.get("/public/govuk_template/assets/stylesheets/images/govuk-crest-2x.png")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val signIn = exec(http("Sign in")
			.post("/login")
			.headers(headers_3)
			.formParam("GovUk_Text_EmailAddress", "${email}")
			.formParam("GovUk_Text_Password", "Genderpaygap1")
			.formParam("ReturnUrl", "${returnUrl}")
			.formParam("button", "login")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				status.in(200),
				regex("Privacy Policy"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"))
			)
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object RegistrationPage {
		val visit = exec(http("Load registration page")
			.get("/create-user-account")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("Create my account"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val register = feed(registrationFeeder)
			.exec(http("Register an account")
			.post("/create-user-account")
			.headers(headers_3)
			.formParam("GovUk_Text_EmailAddress", "${email}-registration")
			.formParam("GovUk_Text_ConfirmEmailAddress", "${email}-registration")
			.formParam("GovUk_Text_FirstName", "Test")
			.formParam("GovUk_Text_LastName", "Example")
			.formParam("GovUk_Text_JobTitle", "Tester")
			.formParam("GovUk_Text_Password", "GenderPayGap123")
			.formParam("GovUk_Text_ConfirmPassword", "GenderPayGap123")
			.formParam("AllowContact", "true")
			.formParam("SendUpdates", "false")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				status.is(200),
				regex("Confirm your email address")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object PrivacyPolicyPage {
		val accept = exec(http("Accept privacy policy")
			.post("/privacy-policy")
			.headers(headers_3)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object SearchForOrganisation {
		val visitManageOrganisationPage = exec(http("Visit manage organisations page")
			.get("/account/organisations")
			.headers(headers_0)
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val chooseAddOrganisation = exec(http("Visit choose organisation sector page")
			.get("/add-organisation/choose-sector")
			.headers(headers_0)
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val chooseOrganisationSector = exec(http("Choose organisation sector (private)")
			.get("/add-organisation/choose-sector?GovUk_Radio_Sector=Private&Validate=True")
			.headers(headers_0)
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val searchForOrganisationName = exec(http("Search for organisation name")
			.get("/add-employer/private/search?query=${organisationName}")
			.headers(headers_0)
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object ManuallyEnterOrganisationDetails {
		val visitOrganisationNamePage = exec(http("Visit manual organisation name page")
			.get("/add-employer/manual/name")
			.headers(headers_0)
			.queryParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterOrganisationName = exec(http("Enter organisation name")
			.get("/add-employer/manual/name")
			.headers(headers_0)
			.queryParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.queryParam("Validate", "True")
			.queryParam("GovUk_Text_OrganisationName", "${organisationName}")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitOrganisationAddressPage = exec(http("Visit manual organisation address page")
			.get("/add-employer/manual/address")
			.headers(headers_0)
			.queryParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.queryParam("OrganisationName", "${organisationName}")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterOrganisationAddress = exec(http("Enter organisation address")
			.get("/add-employer/manual/address")
			.headers(headers_0)
			.queryParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.queryParam("OrganisationName", "${organisationName}")
			.queryParam("Validate", "True")
			.queryParam("GovUk_Text_Address1", "1 Imaginary Street")
			.queryParam("GovUk_Radio_IsUkAddress", "Yes")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitOrganisationSicCodesPage = exec(http("Visit manual organisation SIC codes page")
			.get("/add-employer/manual/sic-codes")
			.headers(headers_0)
			.queryParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.queryParam("OrganisationName", "${organisationName}")
			.queryParam("Address1", "1 Imaginary Street")
			.queryParam("IsUkAddress", "Yes")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterOrganisationSicCodes = exec(http("Enter organisation SIC codes")
			.get("/add-employer/manual/sic-codes")
			.headers(headers_0)
			.queryParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.queryParam("OrganisationName", "${organisationName}")
			.queryParam("Validate", "True")
			.queryParam("Address1", "1 Imaginary Street")
			.queryParam("IsUkAddress", "Yes")
			.queryParam("SicCodes", "41100")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitConfirmOrganisationDetailsPage = exec(http("Visit confirm organisation details page")
			.get("/add-employer/manual/confirm")
			.headers(headers_0)
			.queryParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.queryParam("OrganisationName", "${organisationName}")
			.queryParam("Address1", "1 Imaginary Street")
			.queryParam("IsUkAddress", "Yes")
			.queryParam("SicCodes", "41100")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val confirmManualOrganisationDetails = exec(http("Confirm manual organisation details")
			.post("/add-employer/manual/confirm")
			.headers(headers_0)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.queryParam("OrganisationName", "${organisationName}")
			.queryParam("Validate", "True")
			.queryParam("Address1", "1 Imaginary Street")
			.queryParam("IsUkAddress", "Yes")
			.queryParam("SicCodes", "41100")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object SelectExistingOrganisation {
		val chooseAnOrganisation = exec(http("Choose organisation from results list")
			.get("/add-employer/found?companyNumber=${organisationCompanyNumber}&query=${organisationName}&sector=Private")
			.headers(headers_0)
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val confirmOrganisationChoice = exec(http("Confirm organisation choice")
			.post("/add-employer/found")
			.headers(headers_3)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("CompanyNumber", "${organisationCompanyNumber}")
			.formParam("Query", "${organisationName}")
			.formParam("IsUkAddress", "Yes")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	// Use this if FeatureFlagNewReportingJourney variable is set to true on live
	object ReportGenderPayGapStepByStep {
		val visitManageOrganisation = exec(http("Visit manage organisation")
			.get("/account/organisations")
			.headers(headers_0)
			.check(
				regex("Add or select an employer you're reporting for"),
				css("a[loadtest-id='organisation-link']", "href").find.saveAs("linkToAnOrganisation")
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitOrganisation = exec(http("Visit an organisation page")
			.get("${linkToAnOrganisation}")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("Manage your employer's reporting"),
				regex("for ${organisationName}"),
				css(s"a[loadtest-id='create-report-${CURRENT_YEAR.substring(0, 4)}']", "href").find.saveAs("linkToTheLatestReport")
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitReportOverviewPage = exec(http("Visit report overview page")
			.get("${linkToTheLatestReport}")
			.headers(headers_0)
			.check(
				regex("Reporting as ${organisationName}"),
				regex(s"Enter your gender pay gap data for snapshot date 05 April ${CURRENT_YEAR.substring(0, 4)}")
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterCalculations = exec(http("Enter calculations")
			.post("${linkToAnOrganisation}" + s"/reporting-year-${CURRENT_YEAR.substring(0, 4)}/report/figures")
			.headers(headers_0)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("GovUk_Text_DiffMeanHourlyPayPercent", "50")
			.formParam("GovUk_Text_DiffMedianHourlyPercent", "50")
			.formParam("GovUk_Text_FemaleBonusPayPercent", "50")
			.formParam("GovUk_Text_MaleBonusPayPercent", "50")
			.formParam("GovUk_Text_DiffMeanBonusPercent", "0")
			.formParam("GovUk_Text_DiffMedianBonusPercent", "0")
			.formParam("GovUk_Text_MaleUpperPayBand", "50")
			.formParam("GovUk_Text_FemaleUpperPayBand", "50")
			.formParam("GovUk_Text_MaleUpperMiddlePayBand", "50")
			.formParam("GovUk_Text_FemaleUpperMiddlePayBand", "50")
			.formParam("GovUk_Text_MaleLowerMiddlePayBand", "50")
			.formParam("GovUk_Text_FemaleLowerMiddlePayBand", "50")
			.formParam("GovUk_Text_MaleLowerPayBand", "50")
			.formParam("GovUk_Text_FemaleLowerPayBand", "50")
			.check(
				status.is(200),
				regex(s"Review your gender pay gap data for snapshot date 05 April ${CURRENT_YEAR.substring(0, 4)}")
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterResponsiblePersonDetails = exec(http("Enter responsible person details")
			.post("${linkToAnOrganisation}" + s"/reporting-year-${CURRENT_YEAR.substring(0, 4)}/report/responsible-person")
			.headers(headers_0)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("GovUk_Text_ResponsiblePersonFirstName", "FirstName")
			.formParam("GovUk_Text_ResponsiblePersonLastName", "LastName")
			.formParam("GovUk_Text_ResponsiblePersonJobTitle", "Tester")
			.check(
				status.is(200),
				regex(s"Review your gender pay gap data for snapshot date 05 April ${CURRENT_YEAR.substring(0, 4)}")
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterOrganisationSizeDetails = exec(http("Enter organisation size details")
			.post("${linkToAnOrganisation}" + s"/reporting-year-${CURRENT_YEAR.substring(0, 4)}/report/size-of-organisation")
			.headers(headers_0)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("GovUk_Radio_SizeOfOrganisation", "Employees250To499")
			.check(
				status.is(200),
				regex(s"Review your gender pay gap data for snapshot date 05 April ${CURRENT_YEAR.substring(0, 4)}")
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterLinkToGpgInformationDetails = exec(http("Enter link to GPG information details")
			.post("${linkToAnOrganisation}" + s"/reporting-year-${CURRENT_YEAR.substring(0, 4)}/report/link-to-organisation-website")
			.headers(headers_0)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("GovUk_Text_LinkToOrganisationWebsite", "http://test-employer-information.com/gpg")
			.check(
				status.is(200),
				regex(s"Review your gender pay gap data for snapshot date 05 April ${CURRENT_YEAR.substring(0, 4)}")
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val reviewAndSubmitReport = exec(http("Review and submit report")
			.post("${linkToAnOrganisation}" + s"/reporting-year-${CURRENT_YEAR.substring(0, 4)}/report/submit")
			.headers(headers_0)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				status.is(200),
				regex(s"You've submitted your gender pay gap data for the $CURRENT_YEAR reporting year.")
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object ManageAccount {
		val visit = exec(http("Visit manage account")
			.get("/manage-account")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("Manage your account")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitChangeEmailAddressPage = exec(http("Visit change email address page")
			.get("/manage-account/change-email")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("Change email address"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val changeEmailAddress = exec(http("Change email address")
			.post("/manage-account/change-email")
			.headers(headers_3)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("GovUk_Text_NewEmailAddress", "newemail@example.com")
			.check(
				status.is(200),
				regex("We have sent a verification email to your new email address")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitChangePasswordPage = exec(http("Visit change password page")
			.get("/manage-account/change-password-new")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("Change your password"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val changePassword = exec(http("Change password")
			.post("/manage-account/change-password-new")
			.headers(headers_3)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("GovUk_Text_CurrentPassword", "Genderpaygap1")
			.formParam("GovUk_Text_NewPassword", "Genderpaygap2")
			.formParam("GovUk_Text_ConfirmNewPassword", "Genderpaygap2")
			.check(
				status.is(200),
				regex("Your password has been changed successfully")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitChangePersonalDetailsPage = exec(http("Visit change personal details page")
			.get("/manage-account/change-personal-details")
			.headers(headers_0)
			.check(
				status.is(200),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"),
				regex("Change your personal details"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val changePersonalDetails = exec(http("Change personal details")
			.post("/manage-account/change-personal-details")
			.headers(headers_3)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("GovUk_Text_FirstName", "testing")
			.formParam("GovUk_Text_LastName", "testing")
			.formParam("GovUk_Text_JobTitle", "tester")
			.formParam("GovUk_Text_ContactPhoneNumber", "")
			.check(
				status.is(200),
				regex("Saved changes to personal details")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitChangeContactPreferencesPage = exec(http("Visit change contact preferences page")
			.get("/manage-account/change-contact-preferences")
			.headers(headers_0)
			.check(
				status.is(200),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"),
				regex("Change your contact preferences"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val changeContactPreferences = exec(http("Change contact preferences")
			.post("/manage-account/change-contact-preferences")
			.headers(headers_3)
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.formParam("SendUpdates", "True")
			.formParam("AllowContact", "True")
			.check(
				status.is(200),
				regex("Saved changes to contact preferences")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object Feedback {
		val visit = exec(http("Visit feedback page")
			.get("/send-feedback")
			.headers(headers_0)
			.check(
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"),
				regex("Send us feedback")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val submit = exec(http("Submit a feedback")
			.post("/send-feedback")
			.headers(headers_3)
			.formParam("GovUk_Radio_HowEasyIsThisServiceToUse", "VeryEasy")
			.formParam("GovUk_Checkbox_HowDidYouHearAboutGpg", "NewsArticle")
			.formParam("GovUk_Text_OtherSourceText", "")
			.formParam("GovUk_Checkbox_WhyVisitGpgSite", "FindOutAboutGpg")
			.formParam("GovUk_Text_OtherReasonText", "")
			.formParam("GovUk_Checkbox_WhoAreYou", "EmployeeInterestedInOrganisationData")
			.formParam("GovUk_Text_OtherPersonText", "")
			.formParam("GovUk_Text_Details", "It's a test survey")
			.formParam("EmailAddress", "")
			.formParam("PhoneNumber", "")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(regex("Thank you")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	val userActions = exitBlockOnFail(
		exec(
			HomePage.visit,
			HomePage.search,
			HomePage.visit,
			SignInPage.visit,
			RegistrationPage.visit,
			RegistrationPage.register,
			HomePage.visit,
			SignInPage.visit,
			SignInPage.signIn,
			PrivacyPolicyPage.accept,
			SearchForOrganisation.visitManageOrganisationPage,
			SearchForOrganisation.chooseAddOrganisation,
			SearchForOrganisation.chooseOrganisationSector,
			SearchForOrganisation.searchForOrganisationName,
			ManuallyEnterOrganisationDetails.visitOrganisationNamePage,
			ManuallyEnterOrganisationDetails.enterOrganisationName,
			ManuallyEnterOrganisationDetails.visitOrganisationAddressPage,
			ManuallyEnterOrganisationDetails.enterOrganisationAddress,
			ManuallyEnterOrganisationDetails.visitOrganisationSicCodesPage,
			ManuallyEnterOrganisationDetails.enterOrganisationSicCodes,
			ManuallyEnterOrganisationDetails.visitConfirmOrganisationDetailsPage,
			ManuallyEnterOrganisationDetails.confirmManualOrganisationDetails,
			SearchForOrganisation.visitManageOrganisationPage,
			SearchForOrganisation.chooseAddOrganisation,
			SearchForOrganisation.chooseOrganisationSector,
			SearchForOrganisation.searchForOrganisationName,
			SelectExistingOrganisation.chooseAnOrganisation,
			SelectExistingOrganisation.confirmOrganisationChoice,
			ReportGenderPayGapStepByStep.visitManageOrganisation,
			ReportGenderPayGapStepByStep.visitOrganisation,
			ReportGenderPayGapStepByStep.visitReportOverviewPage,
			ReportGenderPayGapStepByStep.enterCalculations,
			ReportGenderPayGapStepByStep.enterResponsiblePersonDetails,
			ReportGenderPayGapStepByStep.enterOrganisationSizeDetails,
			ReportGenderPayGapStepByStep.enterLinkToGpgInformationDetails,
			ReportGenderPayGapStepByStep.reviewAndSubmitReport,
			ManageAccount.visit,
			ManageAccount.visitChangeEmailAddressPage,
			ManageAccount.changeEmailAddress,
			ManageAccount.visit,
			ManageAccount.visitChangePersonalDetailsPage,
			ManageAccount.changePersonalDetails,
			ManageAccount.visitChangeContactPreferencesPage,
			ManageAccount.changeContactPreferences,
			ManageAccount.visitChangePasswordPage,
			ManageAccount.changePassword,
			Feedback.visit,
			Feedback.submit
		)
	)

	val scn = scenario("Gender pay gap").exec(userActions)

	setUp(scn.inject(
		constantUsersPerSec(1) during (5 hours)
	)).protocols(httpProtocol)
}