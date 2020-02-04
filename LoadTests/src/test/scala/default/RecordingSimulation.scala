package default

import scala.concurrent.duration._
import scala.util.Random

import io.gatling.core.Predef._
import io.gatling.http.Predef._

class RecordingSimulation extends Simulation {

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
		"Origin" -> "https://wa-t1dv-gpg.azurewebsites.net",
		"Upgrade-Insecure-Requests" -> "1")

	val headers_4 = Map(
		"Accept" -> "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
		"DNT" -> "1",
		"Pragma" -> "no-cache",
		"Upgrade-Insecure-Requests" -> "1")

	val headers_5 = Map(
		"DNT" -> "1",
		"Pragma" -> "no-cache")

	val maxNumberOfTestUsers = 10

	val searchFeeder = Iterator.continually(Map("searchCriteria1" -> "tes", "searchCriteria2" -> s"test${Random.nextInt(maxNumberOfTestUsers)+1}"))
	val registrationFeeder = Iterator.continually(Map("email" -> (Random.alphanumeric.take(20).mkString + "@example.com")))
	val signInFeeder = Iterator.continually(Map("email" -> s"user${Random.nextInt(maxNumberOfTestUsers)+1}@example.com"))

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
			.pause(1)

		val search = feed(searchFeeder)
			.exec(http("Search a word first bit")
			.get("/viewing/suggest-employer-name-js?search=${searchCriteria1}")
			.headers(headers_2))
			.pause(1)
			.exec(http("Search the whole word")
			.get("/viewing/suggest-employer-name-js?search=${searchCriteria2}")
			.headers(headers_2)
			.check(
				status.is(200),
				jsonPath("$.Matches[0].Id").find.saveAs("FirstSearchResultId"),
				jsonPath("$.Matches[0].Text").find.saveAs("FirstSearchResultText")))
			.pause(1)
			.exec(http("Select an organisation")
			.get("/employer/${FirstSearchResultId}")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("${FirstSearchResultText}")))
			.pause(1)
	}

	object SignInPage {
		val visit = exec(http("Visit sign in page")
			.get("/manage-organisations")
			.headers(headers_0)
			.check(
				status.in(200, 302),
				regex("Sign in"),
				regex("If you have an account, enter your email address and password"),
				css("input[name='ReturnUrl'","value").saveAs("returnUrl"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken"))
			.resources(http("Load important icon")
				.get("/public/images/icon-important-2x.png"),
				http("Load licence image")
					.get("/account/public/assets/govuk_template/stylesheets/images/open-government-licence_2x.png?0.23.0"),
				http("Load logo")
					.get("/account/public/assets/govuk_template/stylesheets/images/gov.uk_logotype_crown.png?0.23.0"),
				http("Load crest")
					.get("/account/public/assets/govuk_template/stylesheets/images/govuk-crest-2x.png?0.23.0")))
			.pause(1)

		val signIn = feed(signInFeeder)
			.exec(http("Sign in")
			.post("/account/sign-in")
			.headers(headers_3)
			.formParam("Username", "${email}")
			.formParam("Password", "Genderpaygap1")
			.formParam("ReturnUrl", "${returnUrl}")
			.formParam("button", "login")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				css("input[name='id_token']","value").saveAs("idToken"),
				css("input[name='scope']","value").saveAs("signInScope"),
				css("input[name='state']","value").saveAs("signInState"),
				css("input[name='session_state']","value").saveAs("signInSessionState")
			))
			.exec(http("Sign in OIDC")
			.post("/signin-oidc")
			.headers(headers_3)
			.formParam("id_token", "${idToken}")
			.formParam("scope", "${signInScope}")
			.formParam("state", "${signInState}")
			.formParam("session_state", "${signInSessionState}")
			.check(
				regex("Privacy Policy"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")
			))
			.pause(1)

	}

	object RegistrationPage {
		val visit = exec(http("Load registration page")
			.get("/Register/about-you")
			.headers(headers_0)
			.check(
				status.is(200),
				regex("Create an account"),
				css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(1)

		val register = feed(registrationFeeder)
			.exec(http("Register")
			.post("/Register/about-you")
			.headers(headers_3)
			.formParam("EmailAddress", "${email}")
			.formParam("ConfirmEmailAddress", "${email}")
			.formParam("FirstName", "Test")
			.formParam("LastName", "Example")
			.formParam("JobTitle", "Tester")
			.formParam("Password", "GenderPayGap123")
			.formParam("ConfirmPassword", "GenderPayGap123")
			.formParam("AllowContact", "true")
			.formParam("SendUpdates", "false")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(regex("Verify your email address")))
			.pause(1)
	}

	object PrivacyPolicyPage {
		val accept = exec(http("Accept privacy and policy")
			.post("/privacy-policy")
			.headers(headers_3)
			.formParam("command", "Continue")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}")
			.check(
				regex("Manage Organisations"),
				regex("Manage Account")))
  		.pause(1)
	}

	object RegisterOrganisation {
		val visitChooseOrganisationType = exec(http("Visit register an organisation page")
			.get("/Register/organisation-type")
			.headers(headers_0)
			.check(css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(1)

		val chooseOrganisationType = exec(http("Choose organistion type to be private")
			.post("/Register/organisation-type")
			.headers(headers_3)
			.formParam("SearchText", "")
			.formParam("SectorType", "Private")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}"))
			.pause(1)

		val visitSearchForAnOrganisation = exec(http("Visit search an organisation page")
			.get("/Register/organisation-search")
			.headers(headers_0)
			.check(css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(1)

		val searchForAnOrganisation = exec(http("Search for an organisation")
			.post("/Register/organisation-search")
			.headers(headers_3)
			.formParam("SectorType", "Private")
			.formParam("ManualRegistration", "True")
			.formParam("command", "search")
			.formParam("SearchText", "test1")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}"))
			.pause(1)

		val visitChooseAnOrganisation = exec(http("Visit choose an organisation page")
			.get("/Register/choose-organisation")
			.headers(headers_0)
			.check(css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(1)

		val chooseAnOrganisation = exec(http("Choose an organisation")
			.post("/Register/choose-organisation")
			.headers(headers_3)
			.formParam("SectorType", "Private")
			.formParam("SearchText", "test1")
			.formParam("command", "employer_0")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}"))
			.pause(1)

		val visitConfirmOrganisationDetails = exec(http("Visit confirm organisation details page")
			.get("/Register/confirm-organisation")
			.headers(headers_0)
			.check(css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(1)

		val confirmOrganisationDetails = exec(http("Confirm organisation details")
			.post("/Register/confirm-organisation")
			.headers(headers_3)
			.formParam("SearchText", "test1")
			.formParam("SelectedEmployerIndex", "0")
			.formParam("CompanyNumber", "999991")
			.formParam("CharityNumber", "")
			.formParam("MutualNumber", "")
			.formParam("OtherName", "")
			.formParam("OtherValue", "")
			.formParam("SectorType", "Private")
			.formParam("NoReference", "False")
			.formParam("BackAction", "")
			.formParam("AddressReturnAction", "")
			.formParam("ConfirmReturnAction", "ChooseOrganisation")
			.formParam("OrganisationName", "test1")
			.formParam("NameSource", "User")
			.formParam("AddressSource", "User")
			.formParam("SicSource", "User")
			.formParam("SicCodeIds", "2200")
			.formParam("Address1", "test street")
			.formParam("Address2", "")
			.formParam("Address3", "")
			.formParam("City", "")
			.formParam("County", "")
			.formParam("Country", "United Kingdom")
			.formParam("Postcode", "NWN NWN")
			.formParam("PoBox", "")
			.formParam("IsUkAddress", "True")
			.formParam("ContactFirstName", "")
			.formParam("ContactLastName", "")
			.formParam("ContactJobTitle", "")
			.formParam("ContactEmailAddress", "")
			.formParam("ContactPhoneNumber", "")
			.formParam("ManualAddress", "False")
			.formParam("ManualEmployerIndex", "0")
			.formParam("MatchedReferenceCount", "0")
			.formParam("ManualRegistration", "False")
			.formParam("ManualAuthorised", "False")
			.formParam("SelectedAuthorised", "False")
			.formParam("IsFastTrackAuthorised", "False")
			.formParam("command", "confirm")
			.formParam("__RequestVerificationToken", "${requestVerificationToken}"))
			.pause(1)

		val visitPinSent = exec(http("Visit pin sent page")
			.get("/Register/pin-sent")
			.headers(headers_0))
			.pause(1)
	}

	object ManageAccount {
		val visit = exec(http("Visit manage account")
			.get("/manage-account")
			.headers(headers_0))
			.pause(1)

		val visitChangePersonalDetails = exec(http("Visit change personal details")
			.get("/manage-account/change-details")
			.headers(headers_0)
			.check(css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(1)

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
			.formParam("__RequestVerificationToken", "${requestVerificationToken}"))
			.pause(1)
	}

	object Feedback {
		val visit = exec(http("Visit feedback page")
			.get("/send-feedback")
			.headers(headers_0)
			.check(css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(1)

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
			.formParam("__RequestVerificationToken", "${requestVerificationToken}"))
			.pause(8)
	}

	val scn = scenario("Viewing").exec(
		HomePage.visit,
		HomePage.search,
		HomePage.visit,
		SignInPage.visit,
		RegistrationPage.visit,
		RegistrationPage.register,
		HomePage.visit,
		SignInPage.visit,
		SignInPage.signIn,
		PrivacyPolicyPage.accept)

	setUp(scn.inject(rampUsers(3) during (30 seconds))).protocols(httpProtocol)
}