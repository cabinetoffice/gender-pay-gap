package default

import scala.concurrent.duration._
import scala.util.Random

import io.gatling.core.Predef._
import io.gatling.http.Predef._

class RecordingSimulation extends Simulation {

	val httpProtocol = http
		.baseUrl("https://wa-t1dv-gpg.azurewebsites.net")
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

	val searchFeeder = csv("search.csv").random
	val registrationFeeder =  Iterator.continually(Map("email" -> (Random.alphanumeric.take(20).mkString + "@example.com")))

	object HomePage {
		val visit = exec(http("Visit home page")
			.get("/")
			.headers(headers_0)
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
			.exec(http("Search a word")
			.get("/viewing/suggest-employer-name-js?search=${SearchCriteria}")
			.headers(headers_2))
			.pause(1)
			.exec(http("Select an organisation")
			.get("/employer/${EmployerIdentifier}")
			.headers(headers_0))
			.pause(1)

		val startSubmission = exec(http("Start submission")
			.get("/manage-organisations")
			.headers(headers_0)
			.resources(http("Load important icon")
				.get("/public/images/icon-important-2x.png"),
				http("Load licence image")
					.get("/account/public/assets/govuk_template/stylesheets/images/open-government-licence_2x.png?0.23.0"),
				http("Load logo")
					.get("/account/public/assets/govuk_template/stylesheets/images/gov.uk_logotype_crown.png?0.23.0"),
				http("Load crest")
					.get("/account/public/assets/govuk_template/stylesheets/images/govuk-crest-2x.png?0.23.0")))
			.pause(1)
	}

	object SignInPage {
		val startRegistration = exec(http("Load registration page")
			.get("/Register/about-you")
			.headers(headers_0)
			.check(css("input[name='__RequestVerificationToken']", "value").saveAs("requestVerificationToken")))
			.pause(1)
	}

	object RegistrationPage {
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
			.formParam("__RequestVerificationToken", "${requestVerificationToken}"))
			.pause(1)
	}

	object EmailVerificationPage {
		val visit = exec(http("Visit email verification page")
			.get("/Register/verify-email")
			.headers(headers_4)
			.resources(http("Load logo")
				.get("/account/public/assets/govuk_template/stylesheets/images/gov.uk_logotype_crown.png?0.23.0")
				.headers(headers_5),
				http("Load important icon")
					.get("/public/images/icon-important.png")
					.headers(headers_5),
				http("Load licence image")
					.get("/account/public/assets/govuk_template/stylesheets/images/open-government-licence.png?0.23.0")
					.headers(headers_5),
				http("Load crest")
					.get("/account/public/assets/govuk_template/stylesheets/images/govuk-crest.png?0.23.0")
					.headers(headers_5)))
			.pause(1)
	}

	val scn = scenario("Viewing").exec(HomePage.visit, HomePage.search, HomePage.visit, HomePage.startSubmission, SignInPage.startRegistration, RegistrationPage.register, EmailVerificationPage.visit)

	setUp(scn.inject(rampUsers(10) during (1 minute))).protocols(httpProtocol)
}