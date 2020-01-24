package default

import scala.concurrent.duration._

import io.gatling.core.Predef._
import io.gatling.http.Predef._
import io.gatling.jdbc.Predef._

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

	object HomePage {
		val visit = exec(http("Visit home page")
			.get("/")
			.headers(headers_0)
			.resources(http("request_1")
				.get("/public/govuk_template/assets/stylesheets/images/gov.uk_logotype_crown.png")
				.headers(headers_1),
				http("request_2")
					.get("/public/images/search-button.png")
					.headers(headers_1),
				http("request_3")
					.get("/public/govuk_template/assets/stylesheets/images/open-government-licence_2x.png")
					.headers(headers_1),
				http("request_4")
					.get("/public/govuk_template/assets/stylesheets/images/govuk-crest-2x.png")
					.headers(headers_1)))
			.pause(1)

		val search = exec(http("request_5")
			.get("/viewing/suggest-employer-name-js?search=Te")
			.headers(headers_2)
			.resources(http("request_6")
				.get("/viewing/suggest-employer-name-js?search=Tes")
				.headers(headers_2)))
			.pause(1)

		val click = exec(http("request_7")
			.get("/employer/d4JZhnna")
			.headers(headers_0))
			.pause(1)
	}

	val scn = scenario("Viewing").exec(HomePage.visit, HomePage.search)

	setUp(scn.inject(rampUsers(5) during (30 seconds))).protocols(httpProtocol)
}