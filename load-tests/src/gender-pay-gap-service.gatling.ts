import {
    simulation,
    scenario,
    exec,
    css,
    feed,
    rampUsers, substring, jmesPath, ChainBuilder, regex, Session, arrayFeeder
} from "@gatling.io/core";
import {currentLocation, currentLocationRegex, header, http, status} from "@gatling.io/http";
import {authorizationHeader} from './authorization-header';

export default simulation((setUp) => {
	// Pauses are uniform duration between these two:
	const PAUSE_MIN_DURATION = 1;  // seconds
	const PAUSE_MAX_DURATION = 3;  // seconds
    
    const STARTING_ID = 3000000;
    
    const MOST_RECENTLY_COMPLETED_REPORTING_YEAR = 2023;
    const CURRENT_REPORTING_YEAR = 2024;
    
    const RUN_ID = 2;
    
    const DOMAIN_NAME = "dev.gender-pay-gap.service.gov.uk";
    const BASE_URL = "https://" + DOMAIN_NAME;

    const TOTAL_USERS = 1000;
    const TOTAL_TIME = 1000;
    
    const USER_IDS_ARRAY: { USER_ID: number }[] = (function() {
        let array = [];
        for (let userId = 1; userId <= TOTAL_USERS; userId++) {
            array.push({ USER_ID: userId });
        }
        return array;
    })();

    const usersFeeder = arrayFeeder(USER_IDS_ARRAY).circular();

    const httpProtocol = http
        .baseUrl(BASE_URL)
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
    const html_post_headers = {
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        "Origin": BASE_URL,
    };
    
    function newEmailAddressFromUserId(userId: string): string {
        return `loadtest-run-${RUN_ID}-user-${userId}@example.com`;
    }
    function alreadySetupEmailAddressFromUserId(userId: string): string {
        return `loadtest${userId}@example.com`;
    }
    function organisationNameFromUserId(userId: string): string {
        const userIdInt: int = parseInt(userId);
        const organisationId = STARTING_ID + userIdInt;
        return `test_${organisationId}`;
    }
    function percent20(url: string): string {
        return url.replaceAll(' ', '%20');
    }

    const Homepage = {
        visit: (): ChainBuilder =>
            exec(http("Homepage - visit")
                .get("/")
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Search and compare gender pay gap data"),
                )
                .resources(
                    http("Homepage - Load favicon")
                        .get("/assets/images/favicon.ico")
                        .headers(image_get_headers),
                    http("Homepage - Load crown")
                        .get("/assets/images/govuk-apple-touch-icon-180x180.png")
                        .headers(image_get_headers),
                    http("Homepage - Load crest")
                        .get("/assets/images/govuk-crest.png")
                        .headers(image_get_headers),
                    http("Homepage - Load bold font")
                        .get("/assets/images/govuk-crest.png")
                        .headers(font_get_headers),
                    http("Homepage - Load light font")
                        .get("/assets/fonts/light-94a07e06a1-v2.woff2")
                        .headers(font_get_headers),
                )
            ).pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        suggestAutoComplete: (): ChainBuilder =>
            exec(http("Homepage (suggest search box: te)")
                .get("/search/suggest-employer-name-js?search=te")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    header("Content-Type").is("application/json; charset=utf-8"),
                    jmesPath("length(Matches)").ofInt().is(10),
                )
            )
            .exec(http("Homepage (suggest search box: tes)")
                .get("/search/suggest-employer-name-js?search=tes")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    header("Content-Type").is("application/json; charset=utf-8"),
                    jmesPath("length(Matches)").ofInt().is(10),
                )
            )
            .exec(http("Homepage (suggest search box: test)")
                .get("/search/suggest-employer-name-js?search=test")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    header("Content-Type").is("application/json; charset=utf-8"),
                    jmesPath("length(Matches)").ofInt().is(10),
                )
            )
            .exec(http("Homepage (suggest search box: test_)")
                .get("/search/suggest-employer-name-js?search=test_")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    header("Content-Type").is("application/json; charset=utf-8"),
                    jmesPath("length(Matches)").ofInt().is(10),
                )
            ).pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
    }
    
    const SearchAndView = {
        searchPage: (): ChainBuilder =>
            exec(http("Search page - visit")
                .get("/search?EmployerName=test")
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Search by employer name"),
                )
                // .resources()
            )
            .exec(http("Search page - API request 1")
                .get("/search-api?EmployerName=test&Page=0")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    header("Content-Type").is("application/json; charset=utf-8"),
                    jmesPath("length(Employers)").ofInt().is(100),
                ),
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION)
            .exec(http("Search page - API request 2")
                .get("/search-api?EmployerName=test_&Page=0")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    header("Content-Type").is("application/json; charset=utf-8"),
                    jmesPath("length(Employers)").ofInt().is(100),
                ),
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION)
            .exec(http("Search page - API request 3")
                .get("/search-api?EmployerName=test_&Sector=A&Page=0")
                .headers(ajax_get_headers)
                .check(
                    status().is(200),
                    header("Content-Type").is("application/json; charset=utf-8"),
                    jmesPath("length(Employers)").ofInt().gte(10),
                ),
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        viewEmployer: (): ChainBuilder =>
            exec(http("View Employer")
                .get("/employers/5816")
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Gender pay gap reports for"),
                    substring("H M Government Cabinet Office"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        viewReportsForYear: (year: int): ChainBuilder =>
            exec(http(`View Report for Employer - ${year}`)
                .get(`/employers/5816/reporting-year-${year}`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("H M Government Cabinet Office"),
                    regex(`${year}-${(year + 1) % 100}\\s*Gender pay gap report`),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
    }
    
    const Compare = {
        addToCompare: (organisationId: int, employersInComparisonBasket: int): ChainBuilder =>
            exec(http(`Add to Compare - add and redirect - org:${organisationId}`)
                .get(`/compare-employers/add/${organisationId}?returnUrl=%2Femployers%2F${organisationId}`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    currentLocation().is(`${BASE_URL}/employers/${organisationId}`),
                    substring("Your comparison list contains"),
                    regex(`${employersInComparisonBasket}\\s*employer`),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        comparePageDefault: (): ChainBuilder =>
            exec(http("Compare Table - redirect and visit")
                .get(`/compare-employers`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    currentLocation().is(`${BASE_URL}/compare-employers/${MOST_RECENTLY_COMPLETED_REPORTING_YEAR}`),
                    substring(`Comparison for ${MOST_RECENTLY_COMPLETED_REPORTING_YEAR}`),
                )
                // .resources()
            ),
        
        comparePageForYear: (year: int): ChainBuilder =>
            exec(http(`Compare Table For Year - ${year}`)
                .get(`/compare-employers/${year}`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring(`Comparison for ${year}`),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
    }
    
    const CreateAccount = {
        alreadyCreatedAnAccountQuestion: (): ChainBuilder =>
            exec(http("Already Created An Account? - visit")
                .get(`/already-created-an-account-question`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Have you previously created a user account?"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        alreadyCreatedAnAccountAnswer: (): ChainBuilder =>
            exec(http("Already Created An Account? - Answer NO and redirect")
                .get(`/already-created-an-account-question?HaveYouAlreadyCreatedYourUserAccount=No`)
                .headers(html_get_headers)
                .disableFollowRedirect()
                .check(
                    status().is(302),
                    header("Location").is("/create-user-account?isPartOfGovUkReportingJourney=True"),
                )
            ),

        createAccountGet: (): ChainBuilder =>
            exec(http("Create Account - get")
                .get(`/create-user-account`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Create my account"),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        createAccountPost: (): ChainBuilder =>
            exec(http("Create Account - post")
                .post(`/create-user-account`)
                .headers(html_post_headers)
                .formParam("EmailAddress", (session: Session) => newEmailAddressFromUserId(session.get('USER_ID')))
                .formParam("ConfirmEmailAddress", (session: Session) => newEmailAddressFromUserId(session.get('USER_ID')))
                .formParam("FirstName", "Test")
                .formParam("LastName", "Example")
                .formParam("JobTitle", "Tester")
                .formParam("Password", "GenderPayGap123")
                .formParam("ConfirmPassword", "GenderPayGap123")
                .formParam("AllowContact", "true")
                .formParam("SendUpdates", "false")
                .formParam("__RequestVerificationToken", (session: Session) => session.get('REQUEST_VERIFICATION_TOKEN'))
                .check(
                    status().is(200),
                    regex("Confirm your email address")
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
    }

    const Login = {
        loginPageGet: (): ChainBuilder =>
            exec(http("Login Page - redirect and get")
                .get(`/account/organisations`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    currentLocation().is(`${BASE_URL}/login?ReturnUrl=%2Faccount%2Forganisations`),
                    substring("If you have a user account, enter your email address and password."),
                    css("input[name='ReturnUrl'","value").saveAs("RETURN_URL"),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        loginPagePost:(): ChainBuilder =>
            exec(http("Login Page - post and redirect to Privacy Policy")
                .post("/login")
                .headers(html_post_headers)
                .formParam("EmailAddress", (session: Session) => alreadySetupEmailAddressFromUserId(session.get('USER_ID')))
                .formParam("Password", "GenderPayGap123")
                .formParam("ReturnUrl", (session: Session) => session.get('RETURN_URL'))
                .formParam("__RequestVerificationToken", (session: Session) => session.get('REQUEST_VERIFICATION_TOKEN'))
                .check(
                    status().is(200),
                    currentLocation().is(`${BASE_URL}/privacy-policy`),
                    regex("Privacy Policy"),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN")
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        acceptPrivacyPolicyPost:(): ChainBuilder =>
            exec(http("Accept privacy policy")
                .post("/privacy-policy")
                .headers(html_post_headers)
                .formParam("__RequestVerificationToken", (session: Session) => session.get('REQUEST_VERIFICATION_TOKEN'))
                .check(
                    status().is(200),
                    currentLocation().is(`${BASE_URL}/account/organisations`),
                    regex("Add or select an employer you're reporting for"),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN")
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
    }
    
    const AddOrganisation = {
        chooseEmployerTypeQuestion: (): ChainBuilder =>
            exec(http("Add Organisation: Choose Employer Type - question")
                .get(`/add-employer/choose-employer-type`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("What type of employer do you want to add?"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        chooseEmployerTypeAnswer: (): ChainBuilder =>
            exec(http("Add Organisation: Choose Employer Type - answer")
                .get(`/add-employer/choose-employer-type`)
                .queryParam("Validate", "True")
                .queryParam("Sector", "Private")
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    currentLocation().is(`${BASE_URL}/add-employer/private/search`),
                    substring("Find your employer"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        searchByOrganisationName: (): ChainBuilder =>
            exec(http("Add Organisation: Search for organiation by name")
                .get(`/add-employer/public/search`)
                .queryParam("query", (session: Session) => organisationNameFromUserId(session.get('USER_ID')))
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Find your employer"),
                    substring("Your search"),
                    substring("Can't find your employer?"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        manualEnterNameQuestion: (): ChainBuilder =>
            exec(http("Add Organisation: Manual: Employer name - question")
                .get((session: Session) => `/add-employer/manual/name?Sector=Private&Query=${organisationNameFromUserId(session.get("USER_ID"))}`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Employer name"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        manualEnterNameAnswer: (): ChainBuilder =>
            exec(http("Add Organisation: Manual: Employer name - answer")
                .get(`/add-employer/manual/name`)
                .queryParam("Validate", "True")
                .queryParam("Sector", "Private")
                .queryParam("Query", (session: Session) => organisationNameFromUserId(session.get('USER_ID')))
                .queryParam("OrganisationName", (session: Session) => organisationNameFromUserId(session.get('USER_ID')))
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    currentLocation().is((session) => `${BASE_URL}/add-employer/manual/address?Sector=Private&Query=${organisationNameFromUserId(session.get("USER_ID"))}&OrganisationName=${organisationNameFromUserId(session.get("USER_ID"))}`),
                    substring("Registered address of employer"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        manualEnterAddressAnswer: (): ChainBuilder =>
            exec(http("Add Organisation: Manual: Employer address - answer")
                .get(`/add-employer/manual/address`)
                .queryParam("Validate", "True")
                .queryParam("Sector", "Private")
                .queryParam("Query", (session: Session) => organisationNameFromUserId(session.get('USER_ID')))
                .queryParam("OrganisationName", (session: Session) => organisationNameFromUserId(session.get('USER_ID')))
                .queryParam("Address1", "1 Imaginary Street")
                .queryParam("IsUkAddress", "Yes")
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    currentLocation().is((session: Session) => percent20(`${BASE_URL}/add-employer/manual/sic-codes?Sector=Private&Query=${organisationNameFromUserId(session.get('USER_ID'))}&OrganisationName=${organisationNameFromUserId(session.get('USER_ID'))}&Address1=1 Imaginary Street&IsUkAddress=Yes`)),
                    substring("Add a sector code to your employer"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        manualEnterSicCodesAnswer: (): ChainBuilder =>
            exec(http("Add Organisation: Manual: Employer SIC codes - answer")
                .get(`/add-employer/manual/sic-codes`)
                .queryParam("Validate", "True")
                .queryParam("Sector", "Private")
                .queryParam("Query", (session: Session) => organisationNameFromUserId(session.get('USER_ID')))
                .queryParam("OrganisationName", (session: Session) => organisationNameFromUserId(session.get('USER_ID')))
                .queryParam("Address1", "1 Imaginary Street")
                .queryParam("IsUkAddress", "Yes")
                .queryParam("SicCodes", "41100")
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    currentLocation().is((session: Session) => percent20(`${BASE_URL}/add-employer/manual/confirm?Sector=Private&Query=${organisationNameFromUserId(session.get('USER_ID'))}&OrganisationName=${organisationNameFromUserId(session.get('USER_ID'))}&Address1=1 Imaginary Street&IsUkAddress=Yes&SicCodes=41100`)),
                    substring("Confirm your employer's details"),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        manualConfirmPost: (): ChainBuilder =>
            exec(http("Add Organisation: Manual: Confirm answers - POST")
                .post(`/add-employer/manual/confirm`)
                .formParam("Sector", "Private")
                .formParam("Query", (session: Session) => organisationNameFromUserId(session.get('USER_ID')))
                .formParam("OrganisationName", (session: Session) => organisationNameFromUserId(session.get('USER_ID')))
                .formParam("Address1", "1 Imaginary Street")
                .formParam("IsUkAddress", "Yes")
                .formParam("SicCodes", "41100")
                .formParam("__RequestVerificationToken", (session: Session) => session.get('REQUEST_VERIFICATION_TOKEN'))
                .headers(html_post_headers)
                .check(
                    css("h1").find().saveAs("H1_VALUE"),
                    status().is(200),
                    currentLocationRegex("://[^/]*(/[^?]*)").is("/add-employer/confirmation"),
                    substring("We've got your details."),
                    substring("We'll review them and email you to confirm."),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
    }

    const ReportGpgData = {
        manageAllOrganisations: (): ChainBuilder =>
            exec(http("Report: Manage All Organisations page")
                .get(`/account/organisations`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Add or select an employer you're reporting for"),
                    css("a[loadtest-id='organisation-link']", "href")
                        .transform((href) => {
                            const regex = /\/([^/]*)$/;  // Extracts the last part of the URL: the encrypted organisation ID.
                            const match = href.match(regex);
                            return match != null && match.length > 0 ? match[1] : null;
                        })
                        .saveAs("ENCRYPTED_ORGANISATION_ID")
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        manageOrganisation: (): ChainBuilder =>
            exec(http("Report: Manage Organisation page")
                .get((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Manage your employer's reporting"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        startReport: (): ChainBuilder =>
            exec(http("Report: Start page")
                .get((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/start`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Report your gender pay gap data"),
                    substring("Before you start"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        numberOfEmployeesQuestion: (): ChainBuilder =>
            exec(http("Report: Number of Employees - question")
                .get((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/size-of-organisation`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("How many employees did you have on your snapshot date?"),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
        
        numberOfEmployeesAnswer: (): ChainBuilder =>
            exec(http("Report: Number of Employees - answer")
                .post((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/size-of-organisation`)
                .formParam("SizeOfOrganisation", "Employees250To499")
                .formParam("__RequestVerificationToken", (session: Session) => session.get('REQUEST_VERIFICATION_TOKEN'))
                .headers(html_post_headers)
                .check(
                    status().is(200),
                    currentLocation().is((session: Session) =>`${BASE_URL}/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report`),
                    substring("Review your gender pay gap data"),
                    substring("More information is required to complete your submission"),
                )
                // .resources(),
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        figuresQuestion: (): ChainBuilder =>
            exec(http("Report: Figures - question")
                .get((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/figures`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Enter gender pay gap data"),
                    substring("Enter your figures as whole percentages or rounded to one decimal place."),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        figuresAnswer: (): ChainBuilder =>
            exec(http("Report: Figures - answer")
                .post((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/figures`)
                .formParam("DiffMeanHourlyPayPercent", "0")
                .formParam("DiffMedianHourlyPercent", "0")
                .formParam("MaleUpperPayBand", "50")
                .formParam("FemaleUpperPayBand", "50")
                .formParam("MaleUpperMiddlePayBand", "50")
                .formParam("FemaleUpperMiddlePayBand", "50")
                .formParam("MaleLowerMiddlePayBand", "50")
                .formParam("FemaleLowerMiddlePayBand", "50")
                .formParam("MaleLowerPayBand", "50")
                .formParam("FemaleLowerPayBand", "50")
                .formParam("FemaleBonusPayPercent", "100")
                .formParam("MaleBonusPayPercent", "100")
                .formParam("DiffMeanBonusPercent", "0")
                .formParam("DiffMedianBonusPercent", "0")
                .formParam("__RequestVerificationToken", (session: Session) => session.get('REQUEST_VERIFICATION_TOKEN'))
                .headers(html_post_headers)
                .check(
                    status().is(200),
                    currentLocation().is((session: Session) => `${BASE_URL}/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report?initialSubmission=False`),
                    substring("Review your gender pay gap data"),
                    substring("More information is required to complete your submission"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        responsiblePersonQuestion: (): ChainBuilder =>
            exec(http("Report: Responsible Person - question")
                .get((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/responsible-person`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Person responsible in your organisation"),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        responsiblePersonAnswer: (): ChainBuilder =>
            exec(http("Report: Responsible Person - answer")
                .post((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/responsible-person`)
                .formParam("ResponsiblePersonFirstName", "FirstName")
                .formParam("ResponsiblePersonLastName", "LastName")
                .formParam("ResponsiblePersonJobTitle", "Tester")
                .formParam("__RequestVerificationToken", (session: Session) => session.get('REQUEST_VERIFICATION_TOKEN'))
                .headers(html_post_headers)
                .check(
                    status().is(200),
                    currentLocation().is((session: Session) => `${BASE_URL}/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report?initialSubmission=False`),
                    substring("Review your gender pay gap data"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        supportingNarrativeQuestion: (): ChainBuilder =>
            exec(http("Report: Supporting Narrative - question")
                .get((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/link-to-organisation-website`)
                .headers(html_get_headers)
                .check(
                    status().is(200),
                    substring("Provide a link to your supporting narrative"),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        supportingNarrativeAnswer: (): ChainBuilder =>
            exec(http("Report: Supporting Narrative - answer")
                .post((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/link-to-organisation-website`)
                .formParam("LinkToOrganisationWebsite", "https://example.com/gpg")
                .formParam("__RequestVerificationToken", (session: Session) => session.get('REQUEST_VERIFICATION_TOKEN'))
                .headers(html_post_headers)
                .check(
                    status().is(200),
                    currentLocation().is((session: Session) => `${BASE_URL}/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report`),
                    substring("Review your gender pay gap data"),
                    css("input[name='__RequestVerificationToken']", "value").saveAs("REQUEST_VERIFICATION_TOKEN"),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),

        submitPost: (): ChainBuilder =>
            exec(http("Report: Submit - POST")
                .post((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/submit`)
                .formParam("__RequestVerificationToken", (session: Session) => session.get('REQUEST_VERIFICATION_TOKEN'))
                .headers(html_post_headers)
                .check(
                    status().is(200),
                    currentLocationRegex("://[^/]*(/[^?]*)").is((session: Session) => `/account/organisations/${session.get('ENCRYPTED_ORGANISATION_ID')}/reporting-year-${CURRENT_REPORTING_YEAR}/report/confirmation`),
                    substring("You've submitted your gender pay gap data"),
                    substring("Your gender pay gap information has now been published on the Gender pay gap service."),
                )
                // .resources()
            )
            .pause(PAUSE_MIN_DURATION, PAUSE_MAX_DURATION),
    }

    const userActions = feed(usersFeeder).exec(
        // Journey: Search for an employer and view their reports
        Homepage.visit(),
        Homepage.suggestAutoComplete(),

        SearchAndView.searchPage(),

        SearchAndView.viewEmployer(),
        SearchAndView.viewReportsForYear(2020),
        SearchAndView.viewEmployer(),
        SearchAndView.viewReportsForYear(2021),
        SearchAndView.viewEmployer(),
        SearchAndView.viewReportsForYear(2022),

        // Journey: Compare employers
        SearchAndView.searchPage(),
        SearchAndView.viewEmployer(),
        Compare.addToCompare(5816, 1),
        SearchAndView.searchPage(),
        SearchAndView.viewEmployer(),
        Compare.addToCompare(491, 2),
        SearchAndView.searchPage(),
        SearchAndView.viewEmployer(),
        Compare.addToCompare(234, 3),

        Compare.comparePageDefault(),
        Compare.comparePageForYear(2022),
        Compare.comparePageForYear(2021),
        Compare.comparePageForYear(2020),
        SearchAndView.viewReportsForYear(2020),

        // Journey: Create account
        CreateAccount.alreadyCreatedAnAccountQuestion(),
        CreateAccount.alreadyCreatedAnAccountAnswer(),
        CreateAccount.createAccountGet(),
        CreateAccount.createAccountPost(),

        // Journey: Login
        Login.loginPageGet(),
        Login.loginPagePost(),
        Login.acceptPrivacyPolicyPost(),
        
        // Journey: Add an organisation
        AddOrganisation.chooseEmployerTypeQuestion(),
        AddOrganisation.chooseEmployerTypeAnswer(),
        AddOrganisation.searchByOrganisationName(),
        AddOrganisation.manualEnterNameQuestion(),
        AddOrganisation.manualEnterNameAnswer(),
        AddOrganisation.manualEnterAddressAnswer(),
        AddOrganisation.manualEnterSicCodesAnswer(),
        AddOrganisation.manualConfirmPost(),
        
        // Journey: Report GPG data
        ReportGpgData.manageAllOrganisations(),
        ReportGpgData.manageOrganisation(),
        ReportGpgData.startReport(),
        ReportGpgData.numberOfEmployeesQuestion(),
        ReportGpgData.numberOfEmployeesAnswer(),
        ReportGpgData.figuresQuestion(),
        ReportGpgData.figuresAnswer(),
        ReportGpgData.responsiblePersonQuestion(),
        ReportGpgData.responsiblePersonAnswer(),
        ReportGpgData.supportingNarrativeQuestion(),
        ReportGpgData.supportingNarrativeAnswer(),
        ReportGpgData.submitPost()
    );

    const gpgScenario = scenario("Gender Pay Gap scenario").exec(userActions);

    setUp(
        gpgScenario.injectOpen(
            rampUsers(TOTAL_USERS).during(TOTAL_TIME)
        )
    ).protocols(httpProtocol);
});
