import {
    simulation,
    scenario,
    exec,
    csv,
    pause,
    css,
    feed,
    repeat,
    tryMax,
    rampUsers, substring, exitBlockOnFail, jmesPath
} from "@gatling.io/core";
import {http, status} from "@gatling.io/http";
import {authorizationHeader} from './authorization-header';

export default simulation((setUp) => {
    const feeder = csv("search.csv").random();

	// Pauses are uniform duration between these two:
	const PAUSE_MIN_DURATION = 1;  // seconds
	const PAUSE_MAX_DURATION = 10;  // seconds

    const domainName = "dev.gender-pay-gap.service.gov.uk";

    const httpProtocol = http
        .baseUrl("https://" + domainName)
        .inferHtmlResources()
		.authorizationHeader(authorizationHeader)
        .acceptLanguageHeader("en-GB,en;q=0.5")
        .acceptEncodingHeader("gzip, deflate")
        .userAgentHeader("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:134.0) Gecko/20100101 Firefox/134.0");

    const html_get_headers = {
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
    };
    const image_get_headers = {
        "Accept": "image/avif,image/webp,image/png,image/svg+xml,image/*;q=0.8,*/*;q=0.5",
    };
    const font_get_headers = {
        "Accept": "application/font-woff2;q=1.0,application/font-woff;q=0.9,*/*;q=0.8",
    };
    const ajax_get_headers = {
        "Accept": "*/*",
        "X-Requested-With": "XMLHttpRequest"
    };

    // const search = exec(
    //     http("Home").get("/"),
    //     pause(1),
    //     feed(feeder),
    //     http("Search")
    //         .get("/computers?f=#{searchCriterion}")
    //         .check(css("a:contains('#{searchComputerName}')", "href").saveAs("computerUrl")),
    //     pause(1),
    //     http("Select").get("#{computerUrl}").check(status().is(200)),
    //     pause(1)
    // );

    const Homepage = {
        visit:
            exec(http("Homepage (visit)")
                .get("/")
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Search and compare gender pay gap data")
                )
                .resources(
                    http("Homepage (visit) - Load favicon")
                        .get("/assets/images/favicon.ico")
                        .headers(image_get_headers),
                    http("Homepage (visit) - Load crown")
                        .get("/assets/images/govuk-apple-touch-icon-180x180.png")
                        .headers(image_get_headers),
                    http("Homepage (visit) - Load crest")
                        .get("/assets/images/govuk-crest.png")
                        .headers(image_get_headers),
                    http("Homepage (visit) - Load bold font")
                        .get("/assets/images/govuk-crest.png")
                        .headers(font_get_headers),
                    http("Homepage (visit) - Load light font")
                        .get("/assets/fonts/light-94a07e06a1-v2.woff2")
                        .headers(font_get_headers),
                )
            ).pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        suggestAutoComplete:
            exec(http("Homepage (suggest search box: te)")
                .get("/search/suggest-employer-name-js?search=te")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    jmesPath("length(Matches)").ofInt().is(10),
                )
            )
            .exec(http("Homepage (suggest search box: tes)")
                .get("/search/suggest-employer-name-js?search=tes")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    jmesPath("length(Matches)").ofInt().is(10),
                )
            )
            .exec(http("Homepage (suggest search box: test)")
                .get("/search/suggest-employer-name-js?search=test")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    jmesPath("length(Matches)").ofInt().is(10),
                )
            )
            .exec(http("Homepage (suggest search box: test_)")
                .get("/search/suggest-employer-name-js?search=test_")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    jmesPath("length(Matches)").ofInt().is(10),
                )
            ).pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
    }
    
    const SearchAndView = {
        searchPage:
            exec(http("Search page (visit)")
                .get("/search?EmployerName=test")
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Search by employer name")
                )
                .resources(
                    // http("Homepage (visit) - Load favicon")
                    //     .get("/assets/images/favicon.ico")
                    //     .headers(image_get_headers),
                    // http("Homepage (visit) - Load crown")
                    //     .get("/assets/images/govuk-apple-touch-icon-180x180.png")
                    //     .headers(image_get_headers),
                    // http("Homepage (visit) - Load crest")
                    //     .get("/assets/images/govuk-crest.png")
                    //     .headers(image_get_headers),
                    // http("Homepage (visit) - Load bold font")
                    //     .get("/assets/images/govuk-crest.png")
                    //     .headers(font_get_headers),
                    // http("Homepage (visit) - Load light font")
                    //     .get("/assets/fonts/light-94a07e06a1-v2.woff2")
                    //     .headers(font_get_headers),
                )
            ).pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
    }

    // // repeat is a loop resolved at RUNTIME
    // const browse =
    //     // Note how we force the counter name, so we can reuse it
    //     repeat(4, "i").on(http("Page #{i}").get("/computers?p=#{i}"), pause(1));
    //
    // // Note we should be using a feeder here
    // // let's demonstrate how we can retry: let's make the request fail randomly and retry a given
    // // number of times

    // const edit =
    //     // let's try at max 2 times
    //     tryMax(2)
    //         .on(
    //             http("Form").get("/computers/new"),
    //             pause(1),
    //             http("Post")
    //                 .post("/computers")
    //                 .headers(html_get_headers)
    //                 .formParam("name", "Beautiful Computer")
    //                 .formParam("introduced", "2012-05-30")
    //                 .formParam("discontinued", "")
    //                 .formParam("company", "37")
    //                 .check(
    //                     status().is(
    //                         // we do a check on a condition that's been customized with
    //                         // a lambda. It will be evaluated every time a user executes
    //                         // the request
    //                         (session) => 200 + Math.floor(Math.random() * 2) // +0 or +1 at random
    //                     )
    //                 )
    //         )
    //         // if the chain didn't finally succeed, have the user exit the whole scenario
    //         .exitHereIfFailed();

    // const users = scenario("Users").exec(search, browse);
    // const admins = scenario("Admins").exec(search, browse, edit);

    const userActions = exec(
        Homepage.visit,
        Homepage.suggestAutoComplete
    );

    const gpgScenario = scenario("Gender Pay Gap scenario").exec(userActions);

    setUp(
        gpgScenario.injectOpen(
            rampUsers(10).during(10)
        )
    ).protocols(httpProtocol);
});
