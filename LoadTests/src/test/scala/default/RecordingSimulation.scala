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

	val httpProtocol = http
		.baseUrl("https://wa-t1pp-gpg.azurewebsites.net")
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
		"Origin" -> "https://wa-t1pp-gpg.azurewebsites.net",
		"Upgrade-Insecure-Requests" -> "1")

	val headers_4 = Map(
		"Accept" -> "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
		"DNT" -> "1",
		"Pragma" -> "no-cache",
		"Upgrade-Insecure-Requests" -> "1")

	val headers_5 = Map(
		"DNT" -> "1",
		"Pragma" -> "no-cache")

	val searchFeeder = Iterator.continually(Map("searchCriteria1" -> "tes", "searchCriteria2" -> s"test_${MAX_NUM_USERS + Random.nextInt(MAX_NUM_USERS) + 1}"))
	val registrationFeeder = Iterator.continually(Map("email" -> (Random.alphanumeric.take(20).mkString + "@example.com")))
	val usersOrganisationsFeeder = csv("users_organisations.csv").circular

	object HomePage {
		val visit = exec(http("Visit home page")
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
			.exec(http("Search a word first bit")
			.get("/viewing/suggest-employer-name-js?search=${searchCriteria1}")
			.headers(headers_2))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
			.exec(http("Search the whole word")
			.get("/viewing/suggest-employer-name-js?search=${searchCriteria2}")
			.headers(headers_2)
			.check(
				status.is(200),
				jsonPath("$.Matches[0].Id").find.saveAs("FirstSearchResultId"),
				jsonPath("$.Matches[0].Text").find.saveAs("FirstSearchResultText")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
			.exec(http("Select an organisation")
			.get("/employer/${FirstSearchResultId}")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("${FirstSearchResultText}")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object SignInPage {
		val visit = exec(http("Visit sign in page")
			.get("/login?ReturnUrl=${returnUrl}")
			.headers(headers_0)
			.check(
				status.in(200, 302),
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

		val signIn = feed(usersOrganisationsFeeder)
			.exec(http("Sign in")
			.post("/account/sign-in")
			.headers(headers_3)
			.formParam("GovUk_Text_EmailAddress", "${email}")
			.formParam("GovUk_Text_Password", "Genderpaygap1")
			.formParam("ReturnUrl", "${returnUrl}")
			.formParam("button", "login")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			)
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object RegistrationPage {
		val visit = exec(http("Load registration page")
			.get("create-user-account")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("Create my account"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val register = feed(registrationFeeder)
			.exec(http("Register")
			.post("create-user-account")
			.headers(headers_3)
			.formParam("GovUk_Text_EmailAddress", "${email}")
			.formParam("GovUk_Text_ConfirmEmailAddress", "${email}")
			.formParam("GovUk_Text_FirstName", "Test")
			.formParam("GovUk_Text_LastName", "Example")
			.formParam("GovUk_Text_JobTitle", "Tester")
			.formParam("GovUk_Text_Password", "GenderPayGap123")
			.formParam("GovUk_Text_ConfirmPassword", "GenderPayGap123")
			.formParam("AllowContact", "true")
			.formParam("SendUpdates", "false")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(regex("Confirm your email address")))
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
			.get("/add-organisation/private/search?query=${organisationName}")
			.headers(headers_0)
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object ManuallyEnterOrganisationDetails {
		val visitOrganisationNamePage = exec(http("Visit manual organisation name page")
			.get("/add-organisation/manual/name")
			.headers(headers_0)
			.queryParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterOrganisationName = exec(http("Enter organisation name")
			.get("/add-organisation/manual/name")
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
			.get("/add-organisation/manual/address")
			.headers(headers_0)
			.queryParam("Sector", "Private")
			.queryParam("Query", "${organisationName}")
			.queryParam("OrganisationName", "${organisationName}")
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterOrganisationAddress = exec(http("Enter organisation address")
			.get("/add-organisation/manual/address")
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
			.get("/add-organisation/manual/sic-codes")
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
			.get("/add-organisation/manual/sic-codes")
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
			.get("/add-organisation/manual/confirm")
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
			.post("/add-organisation/manual/confirm")
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
			.get("/add-organisation/found?companyNumber=${organisationCompanyNumber}&query=${organisationName}&sector=Private")
			.headers(headers_0)
			.check(
				status.is(200)
			))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val confirmOrganisationChoice = exec(http("Confirm organisation choice")
			.post("/add-organisation/found")
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

	object ReportGenderPayGap {
		val visitManageOrganisation = exec(http("Visit manage organisation")
			.get("/account/organisations")
			.headers(headers_0)
			.check(
				css(session => "a:contains('" + session("organisationName").as[String].toUpperCase() + "')", "href").saveAs("linkToAnOrganisation"),
				regex("Select an organisation")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitOrganisation = exec(http("Visit an organisation page")
			.get("${linkToAnOrganisation}")
			.headers(headers_0)
			.check(
				css("a[id^='NewReport2019']", "href").find.saveAs("linkToTheLatestReport"),
				regex("Manage your organisation's reporting")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val visitEnterReport = exec(http("Visit enter report page")
			.get("${linkToTheLatestReport}")
			.headers(headers_0)
			.check(
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"),
				css("input[name='OrganisationId']", "value").saveAs("organisationId"),
				css("input[name='EncryptedOrganisationId']", "value").saveAs("encryptedOrganisationId"),
				regex("Enter your gender pay gap data for snapshot date")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterCalculation = exec(http("Enter calculation")
			.post("/Submit/enter-calculations")
			.headers(headers_3)
			.formParam("ReturnId", "0")
			.formParam("OrganisationId", "${organisationId}")
			.formParam("EncryptedOrganisationId", "${encryptedOrganisationId}")
			.formParam("ShouldProvideLateReason", "False")
			.formParam("ReportInfo.ReportModifiedDate", "")
			.formParam("ReportInfo.ReportingStartDate", "05/04/2019 00:00:00")
			.formParam("FirstName", "")
			.formParam("JobTitle", "")
			.formParam("LastName", "")
			.formParam("AccountingDate", "05/04/2019 00:00:00")
			.formParam("SectorType", "Private")
			.formParam("OrganisationSize", "NotProvided")
			.formParam("CompanyLinkToGPGInfo", "")
			.formParam("EHRCResponse", "")
			.formParam("LateReason", "")
			.formParam("DiffMeanHourlyPayPercent", "1")
			.formParam("DiffMedianHourlyPercent", "1")
			.formParam("MaleMedianBonusPayPercent", "1")
			.formParam("FemaleMedianBonusPayPercent", "1")
			.formParam("DiffMeanBonusPercent", "1")
			.formParam("DiffMedianBonusPercent", "")
			.formParam("MaleUpperQuartilePayBand", "50")
			.formParam("FemaleUpperQuartilePayBand", "50")
			.formParam("MaleUpperPayBand", "50")
			.formParam("FemaleUpperPayBand", "50")
			.formParam("MaleMiddlePayBand", "50")
			.formParam("FemaleMiddlePayBand", "50")
			.formParam("MaleLowerPayBand", "50")
			.formParam("FemaleLowerPayBand", "50")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"),
				css("input[name='OrganisationId']", "value").saveAs("organisationId"),
				css("input[name='EncryptedOrganisationId']", "value").saveAs("encryptedOrganisationId"),
				regex("Person responsible in your organisation")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterResponsiblePerson = exec(http("Enter responsible person")
			.post("/Submit/person-responsible")
			.headers(headers_3)
			.formParam("ReturnId", "0")
			.formParam("OrganisationId", "${organisationId}")
			.formParam("EncryptedOrganisationId", "${encryptedOrganisationId}")
			.formParam("ShouldProvideLateReason", "False")
			.formParam("ReportInfo.ReportModifiedDate", "")
			.formParam("ReportInfo.ReportingStartDate", "05/04/2019 00:00:00")
			.formParam("DiffMeanBonusPercent", "1.0")
			.formParam("DiffMeanHourlyPayPercent", "1.0")
			.formParam("DiffMedianBonusPercent", "")
			.formParam("DiffMedianHourlyPercent", "1.0")
			.formParam("FemaleLowerPayBand", "50.0")
			.formParam("FemaleMedianBonusPayPercent", "1.0")
			.formParam("FemaleMiddlePayBand", "50.0")
			.formParam("FemaleUpperPayBand", "50.0")
			.formParam("FemaleUpperQuartilePayBand", "50.0")
			.formParam("MaleLowerPayBand", "50.0")
			.formParam("MaleMedianBonusPayPercent", "1.0")
			.formParam("MaleMiddlePayBand", "50.0")
			.formParam("MaleUpperPayBand", "50.0")
			.formParam("MaleUpperQuartilePayBand", "50.0")
			.formParam("AccountingDate", "05/04/2019 00:00:00")
			.formParam("SectorType", "Private")
			.formParam("OrganisationSize", "NotProvided")
			.formParam("CompanyLinkToGPGInfo", "")
			.formParam("EHRCResponse", "")
			.formParam("LateReason", "")
			.formParam("FirstName", "test")
			.formParam("LastName", "test")
			.formParam("JobTitle", "test")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"),
				css("input[name='OrganisationId']", "value").saveAs("organisationId"),
				css("input[name='EncryptedOrganisationId']", "value").saveAs("encryptedOrganisationId"),
				regex("Size of your organisation")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterSizeOfOrganisation = exec(http("Enter size of organisation")
			.post("/Submit/organisation-size")
			.headers(headers_3)
			.formParam("ReturnId", "0")
			.formParam("OrganisationId", "${organisationId}")
			.formParam("EncryptedOrganisationId", "${encryptedOrganisationId}")
			.formParam("ShouldProvideLateReason", "False")
			.formParam("ReportInfo.ReportModifiedDate", "")
			.formParam("ReportInfo.ReportingStartDate", "05/04/2019 00:00:00")
			.formParam("DiffMeanBonusPercent", "1.0")
			.formParam("DiffMeanHourlyPayPercent", "1.0")
			.formParam("DiffMedianBonusPercent", "")
			.formParam("DiffMedianHourlyPercent", "1.0")
			.formParam("FemaleLowerPayBand", "50.0")
			.formParam("FemaleMedianBonusPayPercent", "1.0")
			.formParam("FemaleMiddlePayBand", "50.0")
			.formParam("FemaleUpperPayBand", "50.0")
			.formParam("FemaleUpperQuartilePayBand", "50.0")
			.formParam("MaleLowerPayBand", "50.0")
			.formParam("MaleMedianBonusPayPercent", "1.0")
			.formParam("MaleMiddlePayBand", "50.0")
			.formParam("MaleUpperPayBand", "50.0")
			.formParam("MaleUpperQuartilePayBand", "50.0")
			.formParam("AccountingDate", "05/04/2019 00:00:00")
			.formParam("SectorType", "Private")
			.formParam("OrganisationSize", "2")
			.formParam("CompanyLinkToGPGInfo", "")
			.formParam("EHRCResponse", "")
			.formParam("LateReason", "")
			.formParam("FirstName", "test")
			.formParam("LastName", "test")
			.formParam("JobTitle", "test")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"),
				css("input[name='OrganisationId']", "value").saveAs("organisationId"),
				css("input[name='EncryptedOrganisationId']", "value").saveAs("encryptedOrganisationId"),
				regex("Link to your gender pay gap information")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val enterWebAddress = exec(http("Enter web address")
			.post("/Submit/employer-website")
			.headers(headers_3)
			.formParam("ReturnId", "0")
			.formParam("OrganisationId", "${organisationId}")
			.formParam("EncryptedOrganisationId", "${encryptedOrganisationId}")
			.formParam("ShouldProvideLateReason", "False")
			.formParam("ReportInfo.ReportModifiedDate", "")
			.formParam("ReportInfo.ReportingStartDate", "05/04/2019 00:00:00")
			.formParam("DiffMeanBonusPercent", "1.0")
			.formParam("DiffMeanHourlyPayPercent", "1.0")
			.formParam("DiffMedianBonusPercent", "")
			.formParam("DiffMedianHourlyPercent", "1.0")
			.formParam("FemaleLowerPayBand", "50.0")
			.formParam("FemaleMedianBonusPayPercent", "1.0")
			.formParam("FemaleMiddlePayBand", "50.0")
			.formParam("FemaleUpperPayBand", "50.0")
			.formParam("FemaleUpperQuartilePayBand", "50.0")
			.formParam("MaleLowerPayBand", "50.0")
			.formParam("MaleMedianBonusPayPercent", "1.0")
			.formParam("MaleMiddlePayBand", "50.0")
			.formParam("MaleUpperPayBand", "50.0")
			.formParam("MaleUpperQuartilePayBand", "50.0")
			.formParam("AccountingDate", "05/04/2019 00:00:00")
			.formParam("SectorType", "Private")
			.formParam("OrganisationSize", "Employees250To499")
			.formParam("CompanyLinkToGPGInfo", "http://example.com/")
			.formParam("EHRCResponse", "")
			.formParam("LateReason", "")
			.formParam("FirstName", "test")
			.formParam("LastName", "test")
			.formParam("JobTitle", "test")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"),
				css("input[name='OrganisationId']", "value").saveAs("organisationId"),
				css("input[name='EncryptedOrganisationId']", "value").saveAs("encryptedOrganisationId"),
				regex("Review your gender pay gap data")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val confirmGenderPayGapData = exec(http("Confirm gender pay gap data")
			.post("/Submit/check-data")
			.headers(headers_3)
			.formParam("ReturnId", "0")
			.formParam("OrganisationId", "${organisationId}")
			.formParam("EncryptedOrganisationId", "${encryptedOrganisationId}")
			.formParam("ShouldProvideLateReason", "False")
			.formParam("ReportInfo.Draft", "GenderPayGap.BusinessLogic.Classes.Draft")
			.formParam("ReportInfo.ReportModifiedDate", "")
			.formParam("ReportInfo.ReportingStartDate", "05/04/2019 00:00:00")
			.formParam("DiffMeanBonusPercent", "1.0")
			.formParam("DiffMeanHourlyPayPercent", "1.0")
			.formParam("DiffMedianBonusPercent", "")
			.formParam("DiffMedianHourlyPercent", "1.0")
			.formParam("FemaleLowerPayBand", "50.0")
			.formParam("FemaleMedianBonusPayPercent", "1.0")
			.formParam("FemaleMiddlePayBand", "50.0")
			.formParam("FemaleUpperPayBand", "50.0")
			.formParam("FemaleUpperQuartilePayBand", "50.0")
			.formParam("MaleLowerPayBand", "50.0")
			.formParam("MaleMedianBonusPayPercent", "1.0")
			.formParam("MaleMiddlePayBand", "50.0")
			.formParam("MaleUpperPayBand", "50.0")
			.formParam("MaleUpperQuartilePayBand", "50.0")
			.formParam("AccountingDate", "05/04/2019 00:00:00")
			.formParam("SectorType", "Private")
			.formParam("OrganisationSize", "Employees250To499")
			.formParam("CompanyLinkToGPGInfo", "http://example.com/")
			.formParam("EHRCResponse", "")
			.formParam("LateReason", "")
			.formParam("FirstName", "test")
			.formParam("LastName", "test")
			.formParam("JobTitle", "test")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(regex("You've submitted your gender pay gap data for 2019 to 2020")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)
	}

	object ManageAccount {
		val visit = exec(http("Visit manage account")
			.get("/manage-account")
			.headers(headers_0)
			.check(regex("Manage your account")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		// TODO: Update these - change personal details is now multiple pages
		val visitChangePersonalDetails = exec(http("Visit change personal details")
			.get("/manage-account/change-details")
			.headers(headers_0)
			.check(
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"),
				regex("Change your personal details")))
			.pause(PAUSE_MIN_DUR, PAUSE_MAX_DUR)

		val changePersonalDetails = exec(http("Change personal details")
			.post("/manage-account/change-details")
			.headers(headers_3)
			.formParam("FirstName", "test user")
			.formParam("LastName", "test user")
			.formParam("JobTitle", "test job")
			.formParam("ContactPhoneNumber", "")
			.formParam("AllowContact", "true")
			.formParam("SendUpdates", "false")
			.formParam("AllowContact", "false")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
				.check(regex("Your details have been updated successfully")))
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

	val scn = scenario("Gender pay gap").exec(
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
		ReportGenderPayGap.visitManageOrganisation,
		ReportGenderPayGap.visitOrganisation,
		ReportGenderPayGap.visitEnterReport,
		ReportGenderPayGap.enterCalculation,
		ReportGenderPayGap.enterResponsiblePerson,
		ReportGenderPayGap.enterSizeOfOrganisation,
		ReportGenderPayGap.enterWebAddress,
		ReportGenderPayGap.confirmGenderPayGapData,
		ManageAccount.visit,
		ManageAccount.visitChangePersonalDetails,
		ManageAccount.changePersonalDetails,
		Feedback.visit,
		Feedback.submit)

	setUp(scn.inject(
		constantUsersPerSec(1) during (5 hours)
	)).protocols(httpProtocol)
}