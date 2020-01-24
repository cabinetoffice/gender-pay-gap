package default

import scala.concurrent.duration._

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

	val feeder = csv("search.csv").random

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

		val search = feed(feeder)
			.exec(http("Search a word")
			.get("/viewing/suggest-employer-name-js?search=${SearchCriteria}")
			.headers(headers_2))
			.pause(1)
			.exec(http("Select an organisation")
			.get("/employer/${EmployerIdentifier}")
			.headers(headers_0))
			.pause(1)
	}

	val scn = scenario("Viewing").exec(HomePage.visit, HomePage.search)

	setUp(scn.inject(rampUsers(10) during (30 seconds))).protocols(httpProtocol)
}