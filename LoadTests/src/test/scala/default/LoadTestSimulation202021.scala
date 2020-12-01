package default

import scala.concurrent.duration._

import io.gatling.core.Predef._
import io.gatling.http.Predef._
import io.gatling.jdbc.Predef._

class LoadTestSimulation202021 extends Simulation {

	val httpProtocol = http
		.baseUrl("https://localhost:44371")
		.inferHtmlResources()
		.acceptHeader("*/*")
		.acceptEncodingHeader("gzip, deflate")
		.acceptLanguageHeader("en-US,en;q=0.9")
		.userAgentHeader("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.66 Safari/537.36")

	val headers_0 = Map(
		"accept" -> "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
		"cache-control" -> "no-cache",
		"pragma" -> "no-cache",
		"sec-fetch-dest" -> "document",
		"sec-fetch-mode" -> "navigate",
		"sec-fetch-site" -> "none",
		"sec-fetch-user" -> "?1",
		"upgrade-insecure-requests" -> "1")

	val headers_1 = Map(
		"accept" -> "text/css,*/*;q=0.1",
		"cache-control" -> "no-cache",
		"pragma" -> "no-cache",
		"sec-fetch-dest" -> "style",
		"sec-fetch-mode" -> "no-cors",
		"sec-fetch-site" -> "same-origin")

	val headers_22 = Map(
		"cache-control" -> "no-cache",
		"pragma" -> "no-cache",
		"sec-fetch-dest" -> "script",
		"sec-fetch-mode" -> "no-cors",
		"sec-fetch-site" -> "same-origin")

	val headers_46 = Map(
		"accept" -> "image/avif,image/webp,image/apng,image/*,*/*;q=0.8",
		"cache-control" -> "no-cache",
		"pragma" -> "no-cache",
		"sec-fetch-dest" -> "image",
		"sec-fetch-mode" -> "no-cors",
		"sec-fetch-site" -> "same-origin")

	val headers_63 = Map(
		"cache-control" -> "no-cache",
		"origin" -> "https://localhost:44371",
		"pragma" -> "no-cache",
		"sec-fetch-dest" -> "font",
		"sec-fetch-mode" -> "cors",
		"sec-fetch-site" -> "same-origin")

	val headers_65 = Map(
		"accept" -> "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
		"cache-control" -> "no-cache",
		"pragma" -> "no-cache",
		"sec-fetch-dest" -> "document",
		"sec-fetch-mode" -> "navigate",
		"sec-fetch-site" -> "same-origin",
		"sec-fetch-user" -> "?1",
		"upgrade-insecure-requests" -> "1")

	val headers_79 = Map(
		"accept" -> "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
		"cache-control" -> "no-cache",
		"origin" -> "https://localhost:44371",
		"pragma" -> "no-cache",
		"sec-fetch-dest" -> "document",
		"sec-fetch-mode" -> "navigate",
		"sec-fetch-site" -> "same-origin",
		"sec-fetch-user" -> "?1",
		"upgrade-insecure-requests" -> "1")

	val headers_100 = Map(
		"cache-control" -> "no-cache",
		"pragma" -> "no-cache",
		"sec-fetch-dest" -> "script",
		"sec-fetch-mode" -> "no-cors",
		"sec-fetch-site" -> "cross-site")

	val headers_101 = Map(
		"cache-control" -> "no-cache",
		"content-type" -> "text/plain",
		"origin" -> "https://localhost:44371",
		"pragma" -> "no-cache",
		"sec-fetch-dest" -> "empty",
		"sec-fetch-mode" -> "cors",
		"sec-fetch-site" -> "cross-site")

    val uri2 = "https://www.google-analytics.com"

	val chain_0 = exec(http("Visit home page")
			.get("/")
			.headers(headers_0)
			.resources(http("request_1")
			.get("/public/govuk_template/assets/stylesheets/govuk-template.css?0.12.0")
			.headers(headers_1),
            http("request_2")
			.get("/Content/application.warnings.css")
			.headers(headers_1),
            http("request_3")
			.get("/Content/application.buttons.css")
			.headers(headers_1),
            http("request_4")
			.get("/Content/application.badges.css")
			.headers(headers_1),
            http("request_5")
			.get("/Content/application.cya.css")
			.headers(headers_1),
            http("request_6")
			.get("/Content/site.badges.css")
			.headers(headers_1),
            http("request_7")
			.get("/Content/application.navigation.css")
			.headers(headers_1),
            http("request_8")
			.get("/Content/site.details.css")
			.headers(headers_1),
            http("request_9")
			.get("/Content/Pagination.css")
			.headers(headers_1),
            http("request_10")
			.get("/Content/application.notification.css")
			.headers(headers_1),
            http("request_11")
			.get("/Content/site.charts.css")
			.headers(headers_1),
            http("request_12")
			.get("/Content/site.finder.css")
			.headers(headers_1),
            http("request_13")
			.get("/Content/site.scrolling-headers.css")
			.headers(headers_1),
            http("request_14")
			.get("/Content/Site.css")
			.headers(headers_1),
            http("request_15")
			.get("/Content/site.submissions.css")
			.headers(headers_1),
            http("request_16")
			.get("/Content/cscp.css")
			.headers(headers_1),
            http("request_17")
			.get("/Content/gpg-govuk-table.css")
			.headers(headers_1),
            http("request_18")
			.get("/Content/site.manage-reports.css")
			.headers(headers_1),
            http("request_19")
			.get("/Content/application.css")
			.headers(headers_1),
            http("request_20")
			.get("/public/govuk_template/assets/stylesheets/fonts.css?0.12.0")
			.headers(headers_1),
            http("request_21")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_22")
			.get("/public/govuk_template/assets/javascripts/govuk-template.js?0.13.0")
			.headers(headers_22),
            http("request_23")
			.get("/Scripts/GOVUK/accordion-with-descriptions.js")
			.headers(headers_22),
            http("request_24")
			.get("/Scripts/GOVUK/analytics.js")
			.headers(headers_22),
            http("request_25")
			.get("/Scripts/GOVUK/external-link-tracker.js")
			.headers(headers_22),
            http("request_26")
			.get("/Scripts/GOVUK/details.polyfill.js")
			.headers(headers_22),
            http("request_27")
			.get("/Scripts/GOVUK/govuk-tracker.js")
			.headers(headers_22),
            http("request_28")
			.get("/Scripts/GOVUK/mailto-link-tracker.js")
			.headers(headers_22),
            http("request_29")
			.get("/Scripts/GOVUK/error-tracking.js")
			.headers(headers_22),
            http("request_30")
			.get("/Scripts/GOVUK/print-intent.js")
			.headers(headers_22),
            http("request_31")
			.get("/Scripts/GOVUK/google-analytics-universal-tracker.js")
			.headers(headers_22),
            http("request_32")
			.get("/Scripts/GOVUK/stageprompt.js")
			.headers(headers_22),
            http("request_33")
			.get("/Scripts/GOVUK/modules.js")
			.headers(headers_22),
            http("request_34")
			.get("/Scripts/GOVUK/show-hide-content.js")
			.headers(headers_22),
            http("request_35")
			.get("/assets/javascripts/jquery.tablesorter.min.js")
			.headers(headers_22),
            http("request_36")
			.get("/Scripts/GOVUK/stick-at-top-when-scrolling.js")
			.headers(headers_22),
            http("request_37")
			.get("/Scripts/Application/Collapsible.js")
			.headers(headers_22),
            http("request_38")
			.get("/Scripts/GOVUK/primary-links.js")
			.headers(headers_22),
            http("request_39")
			.get("/Scripts/GOVUK/selection-buttons.js")
			.headers(headers_22),
            http("request_40")
			.get("/Scripts/GOVUK/download-link-tracker.js")
			.headers(headers_22),
            http("request_41")
			.get("/Scripts/GOVUK/shim-links-with-button-role.js")
			.headers(headers_22),
            http("request_42")
			.get("/Scripts/Application/Ajaxify.js")
			.headers(headers_22),
            http("request_43")
			.get("/Scripts/Application/Compare.js")
			.headers(headers_22),
            http("request_44")
			.get("/Scripts/GOVUK/stop-scrolling-at-footer.js")
			.headers(headers_22),
            http("request_45")
			.get("/Scripts/Application/CurrentLocation.js")
			.headers(headers_22),
            http("request_46")
			.get("/public/govuk_template/assets/images/gov.uk_logotype_crown.png?0.13.0")
			.headers(headers_46),
            http("request_47")
			.get("/Scripts/Application/History.js")
			.headers(headers_22),
            http("request_48")
			.get("/Scripts/Application/Site.js")
			.headers(headers_22),
            http("request_49")
			.get("/Scripts/Application/custom-link-tracker.js")
			.headers(headers_22),
            http("request_50")
			.get("/Scripts/Application/LiveSearch.js")
			.headers(headers_22),
            http("request_51")
			.get("/Scripts/Application/details-tracker.js")
			.headers(headers_22),
            http("request_52")
			.get("/Scripts/Application/PreventDoubleClick.js")
			.headers(headers_22),
            http("request_53")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_54")
			.get("/assets/javascripts/jquery-1.11.3.min.js")
			.headers(headers_22),
            http("request_55")
			.get("/Scripts/Application/OptionSelect.js")
			.headers(headers_22),
            http("request_56")
			.get("/Scripts/Application/ShowHide.js")
			.headers(headers_22),
            http("request_57")
			.get("/public/govuk_template/assets/stylesheets/govuk-template-print.css?0.12.0")
			.headers(headers_1),
            http("request_58")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_59")
			.get("/public/govuk_template/assets/stylesheets/images/gov.uk_logotype_crown.png")
			.headers(headers_46),
            http("request_60")
			.get("/assets/images/search-button-white-and-black.png")
			.headers(headers_46),
            http("request_61")
			.get("/public/govuk_template/assets/stylesheets/images/open-government-licence_2x.png")
			.headers(headers_46),
            http("request_62")
			.get("/public/govuk_template/assets/stylesheets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_63")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_64")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63)))
		.pause(12)
		.exec(http("Click submit gender pay gap data")
			.get("/account/organisations")
			.headers(headers_65)
			.resources(http("request_66")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_67")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_68")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_69")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_70")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_71")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63)))
		.pause(4)
		.exec(http("Visit create account page")
			.get("/create-user-account")
			.headers(headers_65)
			.resources(http("request_73")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_74")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_75")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_76")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_77")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_78")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22)))
		.pause(44)
		.exec(http("Enter information to create account")
			.post("/create-user-account")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY3iMvPycQ7czhRP16r55RoQ_9x6dO6XQwhP60TJt0Ly-WQMEK0wo0E1x5iQFLrhLgMZeuWv_dxx1YW4tv7vxV5fD1L5I81-Z-wciLd-jNrN9CnVt_5kfG29kEyikj6Qln0")
			.formParam("GovUk_Text_EmailAddress", "loadtests@softwire.com")
			.formParam("GovUk_Text_ConfirmEmailAddress", "loadtests@softwire.com")
			.formParam("GovUk_Text_FirstName", "John")
			.formParam("GovUk_Text_LastName", "Smith")
			.formParam("GovUk_Text_JobTitle", "Load Tester")
			.formParam("GovUk_Text_Password", "LoadTests1")
			.formParam("GovUk_Text_ConfirmPassword", "LoadTests1")
			.resources(http("request_80")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_81")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_82")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_83")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_84")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_85")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22)))
		.pause(95)
		.exec(http("Visit Manage Organisations")
			.get("/account/organisations")
			.headers(headers_65)
			.resources(http("request_87")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_88")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_89")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_90")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_91")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_92")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22)))
		.pause(13)
		.exec(http("Log in to account after verifying email")
			.post("/login")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY1q6S5-djisd6bCLk6J_q2hfXRuiZBYXX4y9RSU4oad5It8uXhwNGqv9bg7XpxCYM8OfF-hXl7X6AgFMOF2sogWXkODUHkWrhq-pOybwVqIxBSY2WruGUv1sPHWu_CjMRk")
			.formParam("ReturnUrl", "/account/organisations")
			.formParam("GovUk_Text_EmailAddress", "loadtests@softwire.com")
			.formParam("GovUk_Text_Password", "LoadTests1")
			.resources(http("request_94")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_95")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_96")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_97")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_98")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_99")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22)))
		.pause(8)
		.exec(http("request_100")
			.get(uri2 + "/analytics.js")
			.headers(headers_100)
			.resources(http("request_101")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=132215943&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fprivacy-policy&ul=en-us&de=UTF-8&dt=Privacy%20Policy%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=IEBAAEABAAAAAC~&jid=1263185720&gjid=1521067563&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=1238126944")
			.headers(headers_101),
            http("request_102")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=132215943&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fprivacy-policy&ul=en-us&de=UTF-8&dt=Privacy%20Policy%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=IGDACEABBAAAAC~&jid=2142035991&gjid=1703723528&cid=439125182.1606813099&tid=UA-145652997-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=1009972852")
			.headers(headers_101)))
		.pause(145)
		.exec(http("Read and accept privacy policy")
			.post("/privacy-policy")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY3CeOkyGG0_Dwo8Wbn2TPlspkQ9TIuGexQ4ojvwfQJVz1vQ-vQpqb5ukHp_lYIAaD0AzvNSwUy06fFta0XQrD5Hsyn-Okrle0ALSV1cEIHTyhUG9sV4G3HvBcH2yWgiQb5R4PLjuPsmYb5wX-IqwD5tCKnWXd4lpMQW2dWJHWvY8w")
			.resources(http("request_104")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_105")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_106")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_107")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_108")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_109")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_110")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_111")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1059914369&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations&ul=en-us&de=UTF-8&dt=Manage%20your%20organisations%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=91310226&gjid=1423686108&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=1100299042")
			.headers(headers_101)))
		.pause(3)
		.exec(http("request_112")
			.get("/manage-account")
			.headers(headers_65)
			.resources(http("request_113")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_114")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_115")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_116")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_117")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_118")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_119")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_120")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=33519724&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account&ul=en-us&de=UTF-8&dt=Manage%20account%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1657284818")
			.headers(headers_101)))
		.pause(4)
		.exec(http("Change email address")
			.get("/manage-account/change-email")
			.headers(headers_65)
			.resources(http("request_122")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_123")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_124")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_125")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_126")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_127")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_128")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_129")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=835381369&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account%2Fchange-email&ul=en-us&de=UTF-8&dt=Change%20email%20address%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=2119721491")
			.headers(headers_101)))
		.pause(6)
		.exec(http("Confirm change of email address")
			.post("/manage-account/change-email")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY2_SeeMaOLOhQNwc3jGCnSDkMjoCyHyYizoes3ETDZeiQMeYlukjgwerjkDQuDAZQ6SOhS7wu-xmhg15Bo2-861j792KLlpzOIY1CtJAIecHPfm4aKIadej8HVCgIQaeR2CBTRQghkQW_vvOas8u-I3dqd4bPtk3SmnT9vP5z0nqQ")
			.formParam("GovUk_Text_NewEmailAddress", "load-tests@softwire.com")
			.resources(http("request_131")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_132")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_133")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_134")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_135")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_136")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_137")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_138")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1528026794&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account%2Fchange-email&ul=en-us&de=UTF-8&dt=Change%20email%20address%20-%20My%20Account%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=785612435")
			.headers(headers_101)))
		.pause(3)
		.exec(http("Redirect to Manage Account page")
			.get("/manage-account")
			.headers(headers_65)
			.resources(http("request_140")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_141")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_142")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_143")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_144")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_145")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_146")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_147")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=140789773&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account&ul=en-us&de=UTF-8&dt=Manage%20account%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=788933044")
			.headers(headers_101)))
		.pause(47)
		.exec(http("Refresh Manage Account page to see updated email address")
			.get("/manage-account")
			.headers(headers_65)
			.resources(http("request_149")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_150")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_151")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_152")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_153")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_154")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_155")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_156")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=961092241&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account&ul=en-us&de=UTF-8&dt=Manage%20account%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=1637976197&gjid=178151845&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=734925488")
			.headers(headers_101)))
		.pause(4)
		.exec(http("Visit change account password page")
			.get("/manage-account/change-password-new")
			.headers(headers_65)
			.resources(http("request_158")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_159")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_160")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_161")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_162")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_163")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_164")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_165")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1792556042&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account%2Fchange-password-new&ul=en-us&de=UTF-8&dt=Change%20your%20password%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=573694174")
			.headers(headers_101)))
		.pause(21)
		.exec(http("Change account password")
			.post("/manage-account/change-password-new")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY3Dx8ZVKO4a6V5QmnlkiEk_4QUThVEVo3vJQmDsD9K7OPV_clJbHprwKxXnoMZNQoenxZIhShYuHNGyA9tP0UOGHmLvZSCPj06fEPwmK0yaQrXOBHaatq17et8nx4P43OWto_fLqc5s_qkci6_kcXWm-vqpHCuX0HwwrpVA6XrqgQ")
			.formParam("GovUk_Text_CurrentPassword", "LoadTests1")
			.formParam("GovUk_Text_NewPassword", "GpgTest1")
			.formParam("GovUk_Text_ConfirmNewPassword", "GpgTest1")
			.resources(http("request_167")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_168")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_169")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_170")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_171")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_172")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_173")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_174")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1103047526&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account&ul=en-us&de=UTF-8&dt=Manage%20account%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=340800277")
			.headers(headers_101)))
		.pause(3)
		.exec(http("Visit change personal details page")
			.get("/manage-account/change-personal-details")
			.headers(headers_65)
			.resources(http("request_176")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_177")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_178")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_179")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_180")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_181")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_182")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_183")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1903884725&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account%2Fchange-personal-details&ul=en-us&de=UTF-8&dt=Change%20your%20personal%20details%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1664517347")
			.headers(headers_101)))
		.pause(27)
		.exec(http("Change personal details")
			.post("/manage-account/change-personal-details")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY3Yk-6g8JoXy8YoVhEbRi34hhRp9B-4IJeczZsR6lbhPC5ttBGc1gvwYzePTltn5O9fAAM5Rt4nCiEsgVmIUJxkZjeoyivDFAkc_cDRdbhLhLLR6aXAexRRkKP40u7V7RYvAX2R21Bgp036HfvvPriEh-KiMHMr9ClFKDsKYThjgQ")
			.formParam("GovUk_Text_FirstName", "Gary")
			.formParam("GovUk_Text_LastName", "Candles")
			.formParam("GovUk_Text_JobTitle", "Luxury Candle Merchant")
			.formParam("GovUk_Text_ContactPhoneNumber", "07111222333")
			.resources(http("request_185")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_186")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_187")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_188")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_189")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_190")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_191")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_192")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=696171414&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account&ul=en-us&de=UTF-8&dt=Manage%20account%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=501728318")
			.headers(headers_101)))
		.pause(4)
		.exec(http("Visit change contact preferences page")
			.get("/manage-account/change-contact-preferences")
			.headers(headers_65)
			.resources(http("request_194")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_195")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_196")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_197")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_198")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_199")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_200")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_201")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=874606433&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account%2Fchange-contact-preferences&ul=en-us&de=UTF-8&dt=Change%20your%20contact%20preferences%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=280088894&gjid=1253273052&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=1233792591")
			.headers(headers_101)))
		.pause(2)
		.exec(http("Change contact preferences")
			.post("/manage-account/change-contact-preferences")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY3L5uGjkkFYGjc6ZzLyBMa5geei5HSInq9p09vaQbLIcshEtD2n-O7eO0PkkYepivY39Adq9KepICCOgJAweNDeC39HrjEan3qt_Y-DacefotQOelvn0dW2j-mNg2uYuW2kw_q2cfvDRMVEp-iMaYQZKv1CrBpqlgzhknLjdjmbYg")
			.formParam("AllowContact", "True")
			.resources(http("request_203")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_204")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_205")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_206")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_207")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_208")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_209")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_210")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1621928257&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fmanage-account&ul=en-us&de=UTF-8&dt=Manage%20account%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1398872089")
			.headers(headers_101)))
		.pause(3)
		.exec(http("Visit Manage Organisations page")
			.get("/account/organisations")
			.headers(headers_65)
			.resources(http("request_212")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_213")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_214")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_215")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_216")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_217")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_218")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_219")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1352982065&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations&ul=en-us&de=UTF-8&dt=Manage%20your%20organisations%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1719491818")
			.headers(headers_101)))
		.pause(3)
		.exec(http("Add an organisation - choose sector")
			.get("/add-organisation/choose-sector")
			.headers(headers_65)
			.resources(http("request_221")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_222")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_223")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_224")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_225")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_226")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_227")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_228")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1635097629&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fadd-organisation%2Fchoose-sector&ul=en-us&de=UTF-8&dt=What%20type%20of%20organisation%20you%20would%20like%20to%20add%3F%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1122983608")
			.headers(headers_101)))
		.pause(12)
		.exec(http("Add an organisation - choose private sector")
			.get("/add-organisation/choose-sector?GovUk_Radio_Sector=Private&Validate=True")
			.headers(headers_65)
			.resources(http("request_230")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_231")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_232")
			.get("/assets/images/search-button-white-and-black.png")
			.headers(headers_46),
            http("request_233")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_234")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_235")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_236")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_237")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_238")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1235600932&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fadd-organisation%2Fprivate%2Fsearch&ul=en-us&de=UTF-8&dt=Find%20organisation%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=103884814")
			.headers(headers_101)))
		.pause(5)
		.exec(http("Add an organisation - search for an org")
			.get("/add-organisation/private/search?query=candle")
			.headers(headers_65)
			.resources(http("request_240")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_241")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_242")
			.get("/assets/images/search-button-white-and-black.png")
			.headers(headers_46),
            http("request_243")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_244")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_245")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_246")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_247")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_248")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1224698380&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fadd-organisation%2Fprivate%2Fsearch%3Fquery%3Dcandle&ul=en-us&de=UTF-8&dt=Find%20organisation%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=94539596")
			.headers(headers_101)))
		.pause(8)
		.exec(http("request_249")
			.get("/add-organisation/found?companyNumber=12898152&query=candle&sector=Private")
			.headers(headers_65)
			.resources(http("request_250")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_251")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_252")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_253")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_254")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_255")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_256")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_257")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1797657096&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fadd-organisation%2Ffound%3FcompanyNumber%3D12898152%26query%3Dcandle%26sector%3DPrivate&ul=en-us&de=UTF-8&dt=Confirm%20your%20organisation%27s%20details%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1654975953")
			.headers(headers_101)))
		.pause(4)
		.exec(http("Add an organisation - select matching organisation")
			.post("/add-organisation/found")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY1qb-ISvS5nXXy_06tts1u82MKPgk1H9OSWSmWgj1pS4pTCgkXMGN8M-F1ix440D_SAqNUN9-pfpzdLrV6gJFbleP6OOjiHYIzjW08sEELBmvGs05DZAIz1UIuiYO1sBUlEBVHgCatZEB55dXQZcQyvChxeX0YZN85oq4k4iDyS7g")
			.formParam("CompanyNumber", "12898152")
			.formParam("Query", "candle")
			.formParam("IsUkAddress", "Yes")
			.resources(http("request_259")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_260")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_261")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_262")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_263")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_264")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_265")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_266")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=991758243&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fadd-organisation%2Fconfirmation%3FconfirmationId%3D%2CeyZ0yF_9O2l3XNhwAlNMRa_oOntOYZbwx19BqPJZYAc!&ul=en-us&de=UTF-8&dt=We%27re%20sending%20you%20a%20PIN%20by%20post%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=765909501")
			.headers(headers_101)))
		.pause(70)
		.exec(http("Add an organisation - confirm organisation choice")
			.get("/Register/activate-service")
			.headers(headers_65)
			.resources(http("request_268")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_269")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_270")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_271")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_272")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_273")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_274")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_275")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1170879029&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations&ul=en-us&de=UTF-8&dt=Manage%20your%20organisations%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=644601906&gjid=198326481&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=1579265385")
			.headers(headers_101)))
		.pause(3)
		.exec(http("Visit page to enter pin in the post")
			.get("/activate-organisation/,E-peJ0eSXwuuxbbZEtjv7A!!")
			.headers(headers_65)
			.resources(http("request_277")
			.get("/public/govuk_template/assets/stylesheets/govuk-template.css?0.12.0")
			.headers(headers_1),
            http("request_278")
			.get("/Content/application.navigation.css")
			.headers(headers_1),
            http("request_279")
			.get("/Content/application.buttons.css")
			.headers(headers_1),
            http("request_280")
			.get("/Content/application.warnings.css")
			.headers(headers_1),
            http("request_281")
			.get("/Content/application.badges.css")
			.headers(headers_1),
            http("request_282")
			.get("/Content/application.notification.css")
			.headers(headers_1),
            http("request_283")
			.get("/Content/application.cya.css")
			.headers(headers_1),
            http("request_284")
			.get("/Content/Pagination.css")
			.headers(headers_1),
            http("request_285")
			.get("/Content/site.scrolling-headers.css")
			.headers(headers_1),
            http("request_286")
			.get("/Content/site.submissions.css")
			.headers(headers_1),
            http("request_287")
			.get("/Content/site.charts.css")
			.headers(headers_1),
            http("request_288")
			.get("/Content/site.details.css")
			.headers(headers_1),
            http("request_289")
			.get("/Content/site.finder.css")
			.headers(headers_1),
            http("request_290")
			.get("/Content/gpg-govuk-table.css")
			.headers(headers_1),
            http("request_291")
			.get("/Content/site.badges.css")
			.headers(headers_1),
            http("request_292")
			.get("/Content/site.manage-reports.css")
			.headers(headers_1),
            http("request_293")
			.get("/Content/cscp.css")
			.headers(headers_1),
            http("request_294")
			.get("/Content/application.css")
			.headers(headers_1),
            http("request_295")
			.get("/Content/Site.css")
			.headers(headers_1),
            http("request_296")
			.get("/public/govuk_template/assets/stylesheets/fonts.css?0.12.0")
			.headers(headers_1),
            http("request_297")
			.get("/public/govuk_template/assets/javascripts/govuk-template.js?0.13.0")
			.headers(headers_22),
            http("request_298")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_299")
			.get("/Scripts/GOVUK/accordion-with-descriptions.js")
			.headers(headers_22),
            http("request_300")
			.get("/Scripts/GOVUK/download-link-tracker.js")
			.headers(headers_22),
            http("request_301")
			.get("/Scripts/GOVUK/error-tracking.js")
			.headers(headers_22),
            http("request_302")
			.get("/Scripts/GOVUK/analytics.js")
			.headers(headers_22),
            http("request_303")
			.get("/Scripts/GOVUK/details.polyfill.js")
			.headers(headers_22),
            http("request_304")
			.get("/assets/javascripts/jquery.tablesorter.min.js")
			.headers(headers_22),
            http("request_305")
			.get("/Scripts/GOVUK/external-link-tracker.js")
			.headers(headers_22),
            http("request_306")
			.get("/Scripts/GOVUK/modules.js")
			.headers(headers_22),
            http("request_307")
			.get("/Scripts/GOVUK/primary-links.js")
			.headers(headers_22),
            http("request_308")
			.get("/Scripts/GOVUK/govuk-tracker.js")
			.headers(headers_22),
            http("request_309")
			.get("/Scripts/GOVUK/mailto-link-tracker.js")
			.headers(headers_22),
            http("request_310")
			.get("/Scripts/GOVUK/google-analytics-universal-tracker.js")
			.headers(headers_22),
            http("request_311")
			.get("/Scripts/GOVUK/print-intent.js")
			.headers(headers_22),
            http("request_312")
			.get("/Scripts/GOVUK/selection-buttons.js")
			.headers(headers_22),
            http("request_313")
			.get("/Scripts/GOVUK/stageprompt.js")
			.headers(headers_22),
            http("request_314")
			.get("/Scripts/Application/Ajaxify.js")
			.headers(headers_22),
            http("request_315")
			.get("/Scripts/GOVUK/stop-scrolling-at-footer.js")
			.headers(headers_22),
            http("request_316")
			.get("/public/govuk_template/assets/images/gov.uk_logotype_crown.png?0.13.0")
			.headers(headers_46),
            http("request_317")
			.get("/Scripts/GOVUK/shim-links-with-button-role.js")
			.headers(headers_22),
            http("request_318")
			.get("/Scripts/Application/CurrentLocation.js")
			.headers(headers_22),
            http("request_319")
			.get("/Scripts/GOVUK/stick-at-top-when-scrolling.js")
			.headers(headers_22),
            http("request_320")
			.get("/Scripts/Application/Site.js")
			.headers(headers_22),
            http("request_321")
			.get("/Scripts/Application/LiveSearch.js")
			.headers(headers_22),
            http("request_322")
			.get("/Scripts/GOVUK/show-hide-content.js")
			.headers(headers_22),
            http("request_323")
			.get("/Scripts/Application/Compare.js")
			.headers(headers_22),
            http("request_324")
			.get("/Scripts/Application/PreventDoubleClick.js")
			.headers(headers_22),
            http("request_325")
			.get("/Scripts/Application/History.js")
			.headers(headers_22),
            http("request_326")
			.get("/Scripts/Application/OptionSelect.js")
			.headers(headers_22),
            http("request_327")
			.get("/Scripts/Application/custom-link-tracker.js")
			.headers(headers_22),
            http("request_328")
			.get("/Scripts/Application/ShowHide.js")
			.headers(headers_22),
            http("request_329")
			.get("/Scripts/Application/details-tracker.js")
			.headers(headers_22),
            http("request_330")
			.get("/Scripts/Application/Collapsible.js")
			.headers(headers_22),
            http("request_331")
			.get("/assets/javascripts/jquery-1.11.3.min.js")
			.headers(headers_22),
            http("request_332")
			.get("/public/govuk_template/assets/stylesheets/govuk-template-print.css?0.12.0")
			.headers(headers_1),
            http("request_333")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_334")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_335")
			.get("/public/govuk_template/assets/stylesheets/images/gov.uk_logotype_crown.png")
			.headers(headers_46),
            http("request_336")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_337")
			.get("/public/govuk_template/assets/stylesheets/images/open-government-licence_2x.png")
			.headers(headers_46),
            http("request_338")
			.get("/public/govuk_template/assets/stylesheets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_339")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=644437822&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2FRegister%2Factivate-service&ul=en-us&de=UTF-8&dt=Enter%20your%20PIN%20-%20Gender%20pay%20gap%20reporting%20service%20-%20GOV.UK&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1505951674")
			.headers(headers_101)))
		.pause(10)
		.exec(http("Enter pin in the post")
			.post("/Register/activate-service")
			.headers(headers_79)
			.formParam("PIN", "1ED4118")
			.formParam("command", "Activate and continue")
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY1qobkoz7m8AvtJnkMz8c444KHacxTQ0bBzKZkPIiFfMkmvAHJDuqCXMM8Eg_XpKU4RLIGAZFIwCQwFA5lIibzvET5IktrWFeh469fhBy4c02gctm7BktK4o6Yz7y2gxw7mLM7Y4THF4ESVmysvYMkGNEhXs8ohs5sd1OGnZTKb1w")
			.resources(http("request_341")
			.get("/public/govuk_template/assets/stylesheets/govuk-template.css?0.12.0")
			.headers(headers_1),
            http("request_342")
			.get("/Content/application.navigation.css")
			.headers(headers_1),
            http("request_343")
			.get("/Content/application.badges.css")
			.headers(headers_1),
            http("request_344")
			.get("/Content/site.charts.css")
			.headers(headers_1),
            http("request_345")
			.get("/Content/application.buttons.css")
			.headers(headers_1),
            http("request_346")
			.get("/Content/application.warnings.css")
			.headers(headers_1),
            http("request_347")
			.get("/Content/application.notification.css")
			.headers(headers_1),
            http("request_348")
			.get("/Content/application.cya.css")
			.headers(headers_1),
            http("request_349")
			.get("/Content/site.submissions.css")
			.headers(headers_1),
            http("request_350")
			.get("/Content/site.manage-reports.css")
			.headers(headers_1),
            http("request_351")
			.get("/Content/site.scrolling-headers.css")
			.headers(headers_1),
            http("request_352")
			.get("/Content/Pagination.css")
			.headers(headers_1),
            http("request_353")
			.get("/Content/cscp.css")
			.headers(headers_1),
            http("request_354")
			.get("/Content/site.finder.css")
			.headers(headers_1),
            http("request_355")
			.get("/Content/site.details.css")
			.headers(headers_1),
            http("request_356")
			.get("/Content/gpg-govuk-table.css")
			.headers(headers_1),
            http("request_357")
			.get("/Content/Site.css")
			.headers(headers_1),
            http("request_358")
			.get("/Content/site.badges.css")
			.headers(headers_1),
            http("request_359")
			.get("/Content/application.css")
			.headers(headers_1),
            http("request_360")
			.get("/public/govuk_template/assets/stylesheets/fonts.css?0.12.0")
			.headers(headers_1),
            http("request_361")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_362")
			.get("/public/govuk_template/assets/javascripts/govuk-template.js?0.13.0")
			.headers(headers_22),
            http("request_363")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_364")
			.get("/assets/javascripts/jquery.tablesorter.min.js")
			.headers(headers_22),
            http("request_365")
			.get("/Scripts/GOVUK/download-link-tracker.js")
			.headers(headers_22),
            http("request_366")
			.get("/Scripts/GOVUK/external-link-tracker.js")
			.headers(headers_22),
            http("request_367")
			.get("/Scripts/GOVUK/accordion-with-descriptions.js")
			.headers(headers_22),
            http("request_368")
			.get("/Scripts/GOVUK/analytics.js")
			.headers(headers_22),
            http("request_369")
			.get("/Scripts/GOVUK/details.polyfill.js")
			.headers(headers_22),
            http("request_370")
			.get("/Scripts/GOVUK/error-tracking.js")
			.headers(headers_22),
            http("request_371")
			.get("/Scripts/GOVUK/shim-links-with-button-role.js")
			.headers(headers_22),
            http("request_372")
			.get("/Scripts/GOVUK/google-analytics-universal-tracker.js")
			.headers(headers_22),
            http("request_373")
			.get("/Scripts/GOVUK/govuk-tracker.js")
			.headers(headers_22),
            http("request_374")
			.get("/Scripts/GOVUK/mailto-link-tracker.js")
			.headers(headers_22),
            http("request_375")
			.get("/Scripts/GOVUK/modules.js")
			.headers(headers_22),
            http("request_376")
			.get("/Scripts/GOVUK/primary-links.js")
			.headers(headers_22),
            http("request_377")
			.get("/Scripts/GOVUK/print-intent.js")
			.headers(headers_22),
            http("request_378")
			.get("/Scripts/GOVUK/selection-buttons.js")
			.headers(headers_22),
            http("request_379")
			.get("/Scripts/GOVUK/show-hide-content.js")
			.headers(headers_22),
            http("request_380")
			.get("/Scripts/GOVUK/stageprompt.js")
			.headers(headers_22),
            http("request_381")
			.get("/Scripts/GOVUK/stop-scrolling-at-footer.js")
			.headers(headers_22),
            http("request_382")
			.get("/public/govuk_template/assets/images/gov.uk_logotype_crown.png?0.13.0")
			.headers(headers_46),
            http("request_383")
			.get("/Scripts/GOVUK/stick-at-top-when-scrolling.js")
			.headers(headers_22),
            http("request_384")
			.get("/Scripts/Application/Ajaxify.js")
			.headers(headers_22),
            http("request_385")
			.get("/Scripts/Application/CurrentLocation.js")
			.headers(headers_22),
            http("request_386")
			.get("/Scripts/Application/PreventDoubleClick.js")
			.headers(headers_22),
            http("request_387")
			.get("/Scripts/Application/Site.js")
			.headers(headers_22),
            http("request_388")
			.get("/assets/javascripts/jquery-1.11.3.min.js")
			.headers(headers_22),
            http("request_389")
			.get("/Scripts/Application/Collapsible.js")
			.headers(headers_22),
            http("request_390")
			.get("/Scripts/Application/History.js")
			.headers(headers_22),
            http("request_391")
			.get("/Scripts/Application/ShowHide.js")
			.headers(headers_22),
            http("request_392")
			.get("/public/govuk_template/assets/stylesheets/govuk-template-print.css?0.12.0")
			.headers(headers_1),
            http("request_393")
			.get("/Scripts/Application/Compare.js")
			.headers(headers_22),
            http("request_394")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_395")
			.get("/Scripts/Application/LiveSearch.js")
			.headers(headers_22),
            http("request_396")
			.get("/Scripts/Application/OptionSelect.js")
			.headers(headers_22),
            http("request_397")
			.get("/Scripts/Application/custom-link-tracker.js")
			.headers(headers_22),
            http("request_398")
			.get("/Scripts/Application/details-tracker.js")
			.headers(headers_22),
            http("request_399")
			.get("/public/govuk_template/assets/stylesheets/images/gov.uk_logotype_crown.png")
			.headers(headers_46),
            http("request_400")
			.get("/public/govuk_template/assets/stylesheets/images/open-government-licence_2x.png")
			.headers(headers_46),
            http("request_401")
			.get("/public/govuk_template/assets/stylesheets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_402")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_403")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=577091669&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2FRegister%2Fservice-activated&ul=en-us&de=UTF-8&dt=Service%20activated%20-%20Gender%20pay%20gap%20reporting%20service%20-%20GOV.UK&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1424171846")
			.headers(headers_101)))
		.pause(2)
		.exec(http("Visit Manage Organisations page")
			.get("/account/organisations")
			.headers(headers_65)
			.resources(http("request_405")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_406")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_407")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_408")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_409")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_410")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_411")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_412")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=538524240&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations&ul=en-us&de=UTF-8&dt=Manage%20your%20organisations%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1010794571")
			.headers(headers_101)))
		.pause(2)
		.exec(http("Select an organisation to report for")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!")
			.headers(headers_65)
			.resources(http("request_414")
			.get("/public/govuk_template/assets/stylesheets/govuk-template.css?0.12.0")
			.headers(headers_1),
            http("request_415")
			.get("/Content/application.navigation.css")
			.headers(headers_1),
            http("request_416")
			.get("/Content/application.warnings.css")
			.headers(headers_1),
            http("request_417")
			.get("/Content/application.badges.css")
			.headers(headers_1),
            http("request_418")
			.get("/Content/Pagination.css")
			.headers(headers_1),
            http("request_419")
			.get("/Content/application.buttons.css")
			.headers(headers_1),
            http("request_420")
			.get("/Content/application.notification.css")
			.headers(headers_1),
            http("request_421")
			.get("/Content/application.cya.css")
			.headers(headers_1),
            http("request_422")
			.get("/Content/site.badges.css")
			.headers(headers_1),
            http("request_423")
			.get("/Content/site.charts.css")
			.headers(headers_1),
            http("request_424")
			.get("/Content/gpg-govuk-table.css")
			.headers(headers_1),
            http("request_425")
			.get("/Content/site.finder.css")
			.headers(headers_1),
            http("request_426")
			.get("/Content/site.scrolling-headers.css")
			.headers(headers_1),
            http("request_427")
			.get("/Content/site.submissions.css")
			.headers(headers_1),
            http("request_428")
			.get("/Content/cscp.css")
			.headers(headers_1),
            http("request_429")
			.get("/Content/site.details.css")
			.headers(headers_1),
            http("request_430")
			.get("/Content/application.css")
			.headers(headers_1),
            http("request_431")
			.get("/Content/site.manage-reports.css")
			.headers(headers_1),
            http("request_432")
			.get("/Content/Site.css")
			.headers(headers_1),
            http("request_433")
			.get("/public/govuk_template/assets/stylesheets/fonts.css?0.12.0")
			.headers(headers_1),
            http("request_434")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_435")
			.get("/public/govuk_template/assets/javascripts/govuk-template.js?0.13.0")
			.headers(headers_22),
            http("request_436")
			.get("/Scripts/GOVUK/accordion-with-descriptions.js")
			.headers(headers_22),
            http("request_437")
			.get("/Scripts/GOVUK/download-link-tracker.js")
			.headers(headers_22),
            http("request_438")
			.get("/Scripts/GOVUK/error-tracking.js")
			.headers(headers_22),
            http("request_439")
			.get("/Scripts/GOVUK/external-link-tracker.js")
			.headers(headers_22),
            http("request_440")
			.get("/Scripts/GOVUK/details.polyfill.js")
			.headers(headers_22),
            http("request_441")
			.get("/Scripts/GOVUK/analytics.js")
			.headers(headers_22),
            http("request_442")
			.get("/Scripts/GOVUK/google-analytics-universal-tracker.js")
			.headers(headers_22),
            http("request_443")
			.get("/Scripts/GOVUK/modules.js")
			.headers(headers_22),
            http("request_444")
			.get("/assets/javascripts/jquery.tablesorter.min.js")
			.headers(headers_22),
            http("request_445")
			.get("/Scripts/GOVUK/govuk-tracker.js")
			.headers(headers_22),
            http("request_446")
			.get("/Scripts/GOVUK/mailto-link-tracker.js")
			.headers(headers_22),
            http("request_447")
			.get("/Scripts/GOVUK/selection-buttons.js")
			.headers(headers_22),
            http("request_448")
			.get("/Scripts/GOVUK/shim-links-with-button-role.js")
			.headers(headers_22),
            http("request_449")
			.get("/Scripts/GOVUK/primary-links.js")
			.headers(headers_22),
            http("request_450")
			.get("/Scripts/GOVUK/print-intent.js")
			.headers(headers_22),
            http("request_451")
			.get("/Scripts/GOVUK/stageprompt.js")
			.headers(headers_22),
            http("request_452")
			.get("/public/govuk_template/assets/images/gov.uk_logotype_crown.png?0.13.0")
			.headers(headers_46),
            http("request_453")
			.get("/Scripts/GOVUK/stop-scrolling-at-footer.js")
			.headers(headers_22),
            http("request_454")
			.get("/Scripts/Application/PreventDoubleClick.js")
			.headers(headers_22),
            http("request_455")
			.get("/Scripts/Application/History.js")
			.headers(headers_22),
            http("request_456")
			.get("/Scripts/GOVUK/show-hide-content.js")
			.headers(headers_22),
            http("request_457")
			.get("/Scripts/GOVUK/stick-at-top-when-scrolling.js")
			.headers(headers_22),
            http("request_458")
			.get("/Scripts/Application/Collapsible.js")
			.headers(headers_22),
            http("request_459")
			.get("/Scripts/Application/Compare.js")
			.headers(headers_22),
            http("request_460")
			.get("/Scripts/Application/CurrentLocation.js")
			.headers(headers_22),
            http("request_461")
			.get("/Scripts/Application/details-tracker.js")
			.headers(headers_22),
            http("request_462")
			.get("/Scripts/Application/Site.js")
			.headers(headers_22),
            http("request_463")
			.get("/Scripts/Application/ShowHide.js")
			.headers(headers_22),
            http("request_464")
			.get("/Scripts/Application/Ajaxify.js")
			.headers(headers_22),
            http("request_465")
			.get("/Scripts/Application/custom-link-tracker.js")
			.headers(headers_22),
            http("request_466")
			.get("/assets/javascripts/jquery-1.11.3.min.js")
			.headers(headers_22),
            http("request_467")
			.get("/Scripts/Application/LiveSearch.js")
			.headers(headers_22),
            http("request_468")
			.get("/Scripts/Application/OptionSelect.js")
			.headers(headers_22),
            http("request_469")
			.get("/public/govuk_template/assets/stylesheets/govuk-template-print.css?0.12.0")
			.headers(headers_1),
            http("request_470")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_471")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_472")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_473")
			.get("/public/govuk_template/assets/stylesheets/images/gov.uk_logotype_crown.png")
			.headers(headers_46),
            http("request_474")
			.get("/public/govuk_template/assets/stylesheets/images/open-government-licence_2x.png")
			.headers(headers_46),
            http("request_475")
			.get("/public/govuk_template/assets/stylesheets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_476")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=501517708&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fdeclare-scope%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!&ul=en-us&de=UTF-8&dt=We%20need%20more%20information%20-%20Gender%20pay%20gap%20reporting%20service%20-%20GOV.UK&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1872245533")
			.headers(headers_101)))
		.pause(5)
		.exec(http("Declare scope of organisation to report for")
			.post("/declare-scope/,E-peJ0eSXwuuxbbZEtjv7A!!")
			.headers(headers_79)
			.formParam("SnapshotDate", "05/04/2019 00:00:00")
			.formParam("OrganisationName", "CANDLE BOYS LTD")
			.formParam("ScopeStatus", "InScope")
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY2tjyVaTRZCIdzbcYn1bHiSzz4-BS5XZ0OlMlE0j0lFEXPiqAiLrNkopycV6Qgguuejmvb-CULMvVjVnKMQZV8L3_irAvSv5OlhlnNHsJdfpbC_JYDKX6PLs3_i6WkcD5qlY44cP0uReYCNxJZDgIeF9qciuoNjXYcMQKIWT6dciw")
			.resources(http("request_478")
			.get("/public/govuk_template/assets/stylesheets/govuk-template.css?0.12.0")
			.headers(headers_1),
            http("request_479")
			.get("/Content/application.navigation.css")
			.headers(headers_1),
            http("request_480")
			.get("/Content/application.buttons.css")
			.headers(headers_1),
            http("request_481")
			.get("/Content/site.finder.css")
			.headers(headers_1),
            http("request_482")
			.get("/Content/application.warnings.css")
			.headers(headers_1),
            http("request_483")
			.get("/Content/application.badges.css")
			.headers(headers_1),
            http("request_484")
			.get("/Content/application.cya.css")
			.headers(headers_1),
            http("request_485")
			.get("/Content/site.charts.css")
			.headers(headers_1),
            http("request_486")
			.get("/Content/Pagination.css")
			.headers(headers_1),
            http("request_487")
			.get("/Content/site.badges.css")
			.headers(headers_1),
            http("request_488")
			.get("/Content/application.notification.css")
			.headers(headers_1),
            http("request_489")
			.get("/Content/site.scrolling-headers.css")
			.headers(headers_1),
            http("request_490")
			.get("/Content/site.submissions.css")
			.headers(headers_1),
            http("request_491")
			.get("/Content/cscp.css")
			.headers(headers_1),
            http("request_492")
			.get("/Content/gpg-govuk-table.css")
			.headers(headers_1),
            http("request_493")
			.get("/Content/site.manage-reports.css")
			.headers(headers_1),
            http("request_494")
			.get("/Content/site.details.css")
			.headers(headers_1),
            http("request_495")
			.get("/Content/Site.css")
			.headers(headers_1),
            http("request_496")
			.get("/Content/application.css")
			.headers(headers_1),
            http("request_497")
			.get("/public/govuk_template/assets/stylesheets/fonts.css?0.12.0")
			.headers(headers_1),
            http("request_498")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_499")
			.get("/public/govuk_template/assets/javascripts/govuk-template.js?0.13.0")
			.headers(headers_22),
            http("request_500")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_501")
			.get("/Scripts/GOVUK/analytics.js")
			.headers(headers_22),
            http("request_502")
			.get("/Scripts/GOVUK/accordion-with-descriptions.js")
			.headers(headers_22),
            http("request_503")
			.get("/assets/javascripts/jquery.tablesorter.min.js")
			.headers(headers_22),
            http("request_504")
			.get("/Scripts/GOVUK/download-link-tracker.js")
			.headers(headers_22),
            http("request_505")
			.get("/Scripts/GOVUK/error-tracking.js")
			.headers(headers_22),
            http("request_506")
			.get("/Scripts/GOVUK/external-link-tracker.js")
			.headers(headers_22),
            http("request_507")
			.get("/Scripts/GOVUK/details.polyfill.js")
			.headers(headers_22),
            http("request_508")
			.get("/Scripts/GOVUK/govuk-tracker.js")
			.headers(headers_22),
            http("request_509")
			.get("/Scripts/GOVUK/selection-buttons.js")
			.headers(headers_22),
            http("request_510")
			.get("/Scripts/GOVUK/shim-links-with-button-role.js")
			.headers(headers_22),
            http("request_511")
			.get("/Scripts/GOVUK/google-analytics-universal-tracker.js")
			.headers(headers_22),
            http("request_512")
			.get("/Scripts/GOVUK/mailto-link-tracker.js")
			.headers(headers_22),
            http("request_513")
			.get("/Scripts/GOVUK/print-intent.js")
			.headers(headers_22),
            http("request_514")
			.get("/Scripts/GOVUK/show-hide-content.js")
			.headers(headers_22),
            http("request_515")
			.get("/Scripts/GOVUK/stageprompt.js")
			.headers(headers_22),
            http("request_516")
			.get("/Scripts/GOVUK/modules.js")
			.headers(headers_22),
            http("request_517")
			.get("/Scripts/GOVUK/stick-at-top-when-scrolling.js")
			.headers(headers_22),
            http("request_518")
			.get("/Scripts/GOVUK/stop-scrolling-at-footer.js")
			.headers(headers_22),
            http("request_519")
			.get("/Scripts/Application/Site.js")
			.headers(headers_22),
            http("request_520")
			.get("/Scripts/Application/CurrentLocation.js")
			.headers(headers_22),
            http("request_521")
			.get("/Scripts/Application/PreventDoubleClick.js")
			.headers(headers_22),
            http("request_522")
			.get("/Scripts/GOVUK/primary-links.js")
			.headers(headers_22),
            http("request_523")
			.get("/Scripts/Application/History.js")
			.headers(headers_22),
            http("request_524")
			.get("/Scripts/Application/ShowHide.js")
			.headers(headers_22),
            http("request_525")
			.get("/Scripts/Application/Collapsible.js")
			.headers(headers_22),
            http("request_526")
			.get("/public/govuk_template/assets/images/gov.uk_logotype_crown.png?0.13.0")
			.headers(headers_46),
            http("request_527")
			.get("/assets/javascripts/jquery-1.11.3.min.js")
			.headers(headers_22),
            http("request_528")
			.get("/Scripts/Application/Compare.js")
			.headers(headers_22),
            http("request_529")
			.get("/Scripts/Application/custom-link-tracker.js")
			.headers(headers_22),
            http("request_530")
			.get("/Scripts/Application/details-tracker.js")
			.headers(headers_22),
            http("request_531")
			.get("/Scripts/Application/Ajaxify.js")
			.headers(headers_22),
            http("request_532")
			.get("/Scripts/Application/LiveSearch.js")
			.headers(headers_22),
            http("request_533")
			.get("/Scripts/Application/OptionSelect.js")
			.headers(headers_22),
            http("request_534")
			.get("/public/govuk_template/assets/stylesheets/govuk-template-print.css?0.12.0")
			.headers(headers_1),
            http("request_535")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_536")
			.get("/public/govuk_template/assets/stylesheets/images/gov.uk_logotype_crown.png")
			.headers(headers_46),
            http("request_537")
			.get("/public/govuk_template/assets/stylesheets/images/open-government-licence_2x.png")
			.headers(headers_46),
            http("request_538")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_539")
			.get("/public/govuk_template/assets/stylesheets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_540")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=184341243&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Fdeclare-scope%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!&ul=en-us&de=UTF-8&dt=We%20need%20more%20information%20-%20Gender%20pay%20gap%20reporting%20service%20-%20GOV.UK&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=2111328054")
			.headers(headers_101)))
		.pause(3)
		.exec(http("View organisation details")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!")
			.headers(headers_65)
			.resources(http("request_542")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_543")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_544")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_545")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_546")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_547")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_548")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_549")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1819755368&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!&ul=en-us&de=UTF-8&dt=Manage%20your%20organisations%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1491012567")
			.headers(headers_101)))
		.pause(20)
		.exec(http("Create report for organisation")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report")
			.headers(headers_65)
			.resources(http("request_551")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_552")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_553")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_554")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_555")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_556")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_557")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_558")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=964260320&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1849337194")
			.headers(headers_101)))
		.pause(9)
		.exec(http("Visit hourly pay page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/hourly-pay")
			.headers(headers_65)
			.resources(http("request_560")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_561")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_562")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_563")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_564")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_565")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_566")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_567")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=247805794&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Fhourly-pay&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=1666268957&gjid=127871700&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=1466340310")
			.headers(headers_101)))
		.pause(28)
		.exec(http("Enter hourly pay figures")
			.post("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/hourly-pay")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY0_gNavBJcnVJBzxeBItItF_EevwyHJ7UPaXCdCO3evIZf3gfGVofi_Ofm8aG7Zc9f_5wWFt1rqC6a6ZKITew7zFZxLTHSihxlO5YJn9UTg6Ce2xviXrUOK5eYOsDvtiDWECqirs1QJe19v1e5Cc4AbxF2qJwNToOyVUJSFU-cOzA")
			.formParam("GovUk_Text_DiffMeanHourlyPayPercent", "1.5")
			.formParam("GovUk_Text_DiffMedianHourlyPercent", "3.8")
			.formParam("Action", "SaveAndContinue")
			.resources(http("request_569")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_570")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_571")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_572")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_573")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_574")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_575")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_576")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1355258453&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1730146563")
			.headers(headers_101)))
		.pause(7)
		.exec(http("Visit bonus pay page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/bonus-pay")
			.headers(headers_65)
			.resources(http("request_578")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_579")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_580")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_581")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_582")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_583")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_584")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_585")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=982606302&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Fbonus-pay&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1477254601")
			.headers(headers_101)))
		.pause(32)
		.exec(http("Enter bonus pay figures")
			.post("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/bonus-pay")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY2aaUOhK2ep_bF0tInXTpPLIRQqQD6txNfQDg2rmZVD7re0GXdrCZIOcILIxL4ftR_o7KTkGeJ1trLEmqgmvSt4R15NHCHHude0Im9aMvgAqg-1-_106ycIfnFXfKhvC3SAOEJDhtiqLJYR2QrvBBCXG9Kv0bDThmnmvRuO9Gruaw")
			.formParam("GovUk_Text_FemaleBonusPayPercent", "15.0")
			.formParam("GovUk_Text_MaleBonusPayPercent", "9.5")
			.formParam("GovUk_Text_DiffMeanBonusPercent", "-2.4")
			.formParam("GovUk_Text_DiffMedianBonusPercent", "-0.8")
			.formParam("Action", "SaveAndContinue")
			.resources(http("request_587")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_588")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_589")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_590")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_591")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_592")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_593")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_594")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=2088256053&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=664771906&gjid=33597620&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=333519781")
			.headers(headers_101)))
		.pause(5)
		.exec(http("Visit employees by pay quartile page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/employees-by-pay-quartile")
			.headers(headers_65)
			.resources(http("request_596")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_597")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_598")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_599")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_600")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_601")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_602")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_603")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1916130178&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Femployees-by-pay-quartile&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=204826691")
			.headers(headers_101)))
		.pause(15)
		.exec(http("Enter employees by pay quartile figures")
			.post("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/employees-by-pay-quartile")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY32Kud9lCiWtdPzDRIPS9zbkaY7SLkXSJrUJPBXA2anQm43Qy2IVViC5qUradBDd8vpO426KsppqolHv7QhbCMEvhwIl7eZihrDSF7tjSTmTR82r3D2V4kjeptNPE7aufNmAic4_MK7uQpaMVVOVOJ_6pGJhkraIbMwcpd6xtfDRA")
			.formParam("GovUk_Text_MaleUpperPayBand", "53")
			.formParam("GovUk_Text_FemaleUpperPayBand", "47")
			.formParam("GovUk_Text_MaleUpperMiddlePayBand", "62")
			.formParam("GovUk_Text_FemaleUpperMiddlePayBand", "38")
			.formParam("GovUk_Text_MaleLowerMiddlePayBand", "33")
			.formParam("GovUk_Text_FemaleLowerMiddlePayBand", "67")
			.formParam("GovUk_Text_MaleLowerPayBand", "82")
			.formParam("GovUk_Text_FemaleLowerPayBand", "18")
			.formParam("Action", "SaveAndContinue")
			.resources(http("request_727")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_728")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_729")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_730")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_731")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_732")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_733")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_734")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1918836482&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=820271198")
			.headers(headers_101)))
		.pause(4)
		.exec(http("Visit responsible person page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/responsible-person")
			.headers(headers_65)
			.resources(http("request_736")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_737")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_738")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_739")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_740")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_741")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_742")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_743")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1016130576&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Fresponsible-person&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1910636097")
			.headers(headers_101)))
		.pause(15)
		.exec(http("Enter responsible person details")
			.post("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/responsible-person")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY3343m63AHbK8D02FAC5fyuaqsogai6NFXng_NtPp-xfcUyvZ-UQeujTKiDtwndaaiAadR1ybTdghKhryvYVzVAIS9Gwhh_hU4xcZAvTE2ijFEe5bbtmTHRhrE1_-bT_OUGU7kfTuP2MaMr2HZOHmWegry8m2jNvXA--W0vfe0lNw")
			.formParam("GovUk_Text_ResponsiblePersonFirstName", "Gareth")
			.formParam("GovUk_Text_ResponsiblePersonLastName", "Chandelier")
			.formParam("GovUk_Text_ResponsiblePersonJobTitle", "Candle Merchant")
			.formParam("Action", "SaveAndContinue")
			.resources(http("request_745")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_746")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_747")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_748")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_749")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_750")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_751")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_752")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1154545056&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=664088567&gjid=2101177148&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=2117091321")
			.headers(headers_101)))
		.pause(3)
		.exec(http("Visit size of organisation page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/size-of-organisation")
			.headers(headers_65)
			.resources(http("request_754")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_755")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_756")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_757")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_758")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_759")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_760")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_761")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=2113871749&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Fsize-of-organisation&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1417306791")
			.headers(headers_101)))
		.pause(13)
		.exec(http("Enter size of organisation details")
			.post("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/size-of-organisation")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY3RkN8QwDMdT-a-nB5Z0cNRAK5RgzoN0uWWSpo_ItMCng4oieMayWYCiDO8aI8_rJM1JB2_soGR7HpL0-FJcUzDrq71T77Lsmf1nWwjzWElhGxsIpep1wp2hAS2ve11W1pO7BAu9kAntCfITghWq9X3CA9wyvdCpYDg195wIez6Yg")
			.formParam("GovUk_Radio_SizeOfOrganisation", "Employees250To499")
			.formParam("Action", "SaveAndContinue")
			.resources(http("request_763")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_764")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_765")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_766")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_767")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_768")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_769")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_770")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=97494054&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=145642955")
			.headers(headers_101)))
		.pause(6)

val chain_1 = exec(http("Visit link to organisation website page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/link-to-organisation-website")
			.headers(headers_65)
			.resources(http("request_772")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_773")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_774")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_775")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_776")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_777")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_778")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_779")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=550108424&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Flink-to-organisation-website&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1231792157")
			.headers(headers_101)))
		.pause(9)
		.exec(http("Skip adding link to organisation website")
			.post("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/link-to-organisation-website")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY2FmOEI1uxspcLlyGkn-I7U_L6cYCx0W0kQ37VctvGKqCpASFyFhD_TE4CASK2OJaH-HazCdCHbG-dgiQj1mdTcO5Jq2f5GuJEnOQml4h1qEsb7QLZgO22Mq31VeMzSP2k9GwOAieHsdne9RtTR5J2M7t9ibyYqSmeV3p4hrpP9Ew")
			.formParam("GovUk_Text_LinkToOrganisationWebsite", "")
			.formParam("Action", "SaveAndContinue")
			.resources(http("request_781")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_782")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_783")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_784")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_785")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_786")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_787")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_788")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=965311399&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=729071429")
			.headers(headers_101)))
		.pause(4)
		.exec(http("Visit review and submit page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/review-and-submit")
			.headers(headers_65)
			.resources(http("request_790")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_791")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_792")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_793")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_794")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_795")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_796")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_797")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=955076072&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Freview-and-submit&ul=en-us&de=UTF-8&dt=Report%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=505114236")
			.headers(headers_101)))
		.pause(26)
		.exec(http("Review and submit gender pay gap report")
			.post("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/review-and-submit")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY0gC6wjkaBCFk1Uc6urckMoWR9Z9TkGVikF6scSd-bbNcfvLYgBHNF9vHAOzIZHA8gFEYw18hjmatNVFAjzABHese1p5fPSWiBTXgIA8Dk_uJ3VWiUJgnMFKbew4ZZJ_1YaK5FKkuTEaHM9mTK5Z2w_X2X7TIfXTmyVHS64Ekonuw")
			.resources(http("request_799")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_800")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_801")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_802")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_803")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_804")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_805")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_806")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1035495023&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Fconfirmation%3FconfirmationId%3D%2CSWD1mS_8mO1YWALKEBjvsQ!!&ul=en-us&de=UTF-8&dt=You%27ve%20reported%20your%20gender%20pay%20gap%20data%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=1559591829&gjid=1783451925&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=1415569423")
			.headers(headers_101)))
		.pause(4)
		.exec(http("Visit organisation details page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!")
			.headers(headers_65)
			.resources(http("request_808")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_809")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_810")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_811")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_812")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_813")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_814")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_815")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1683771524&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!&ul=en-us&de=UTF-8&dt=Manage%20your%20organisations%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1221826662")
			.headers(headers_101)))
		.pause(5)
		.exec(http("Visit page to edit gender pay gap report")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report")
			.headers(headers_65)
			.resources(http("request_817")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_818")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_819")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_820")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_821")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_822")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_823")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_824")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1148771672&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport&ul=en-us&de=UTF-8&dt=Edit%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=2013145112")
			.headers(headers_101)))
		.pause(3)
		.exec(http("Visit add link to organisation website page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/link-to-organisation-website")
			.headers(headers_65)
			.resources(http("request_826")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_827")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_828")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_829")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_830")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_831")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_832")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_833")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1089693946&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Flink-to-organisation-website&ul=en-us&de=UTF-8&dt=Edit%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=732119854")
			.headers(headers_101)))
		.pause(9)
		.exec(http("Provide link to organisation website")
			.post("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/link-to-organisation-website")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY35CJ4vPL2OHgzfpQVABz4rgBnH6B1S6KkOLE-FA-8T3pYPWhnlqpjhXMZb1DTW47CZhdAGuQGqp405rqYTe5Ce68b6MKYhCm4TkyaCvMojRj9eplsLA6K2wSabm1GC5UUEqeeG7t4z-ntm1wvsXWIy0ieNajGrz3UG8ObHjjyMfA")
			.formParam("GovUk_Text_LinkToOrganisationWebsite", "http://google.com")
			.formParam("Action", "SaveAndContinue")
			.resources(http("request_835")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_836")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_837")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_838")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_839")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_840")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_841")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_842")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=878473691&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport&ul=en-us&de=UTF-8&dt=Edit%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=507019476")
			.headers(headers_101)))
		.pause(3)
		.exec(http("Visit review and submit page")
			.get("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/review-and-submit")
			.headers(headers_65)
			.resources(http("request_844")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_845")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_846")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_847")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_848")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_849")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_850")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_851")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=137813611&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Freview-and-submit&ul=en-us&de=UTF-8&dt=Edit%20your%20gender%20pay%20gap%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1907568069")
			.headers(headers_101)))
		.pause(4)
		.exec(http("Review and submit gender pay gap report")
			.post("/account/organisations/,E-peJ0eSXwuuxbbZEtjv7A!!/reporting-year-2020/report/review-and-submit")
			.headers(headers_79)
			.formParam("__RequestVerificationToken", "CfDJ8BksbxbmDG1DrEuoH14LfY2lz47rx1D170SyQR9dsBk2lgtE4m-CuNIxUCRHtQlFK-IqdxqYkalnqhjIzw8tM6ETmw3ZQDe7968kpvBaIOpFuCzG65XTOWuqBv33sl9XYgHYFe2mbG_IUGHWaHyWzeggQAXTz3zaQi0Gl4v1n2gjj5BogN8W7NcXHYEUnDCsAg")
			.resources(http("request_853")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_854")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_855")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_856")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_857")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_858")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_859")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_860")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=843102581&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Faccount%2Forganisations%2F%2CE-peJ0eSXwuuxbbZEtjv7A!!%2Freporting-year-2020%2Freport%2Fconfirmation%3FconfirmationId%3D%2CYg_tK4vO2FmTkQ1qb3s_qg!!&ul=en-us&de=UTF-8&dt=You%27ve%20reported%20your%20gender%20pay%20gap%20data%20-%20reporting%20year%202020-21%20-%20CANDLE%20BOYS%20LTD%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1487471079")
			.headers(headers_101)))
		.pause(7)
		.exec(http("Visit actions to close the gap page")
			.get("/actions-to-close-the-gap")
			.headers(headers_65)
			.resources(http("request_862")
			.get("/img/what-works.png")
			.headers(headers_46),
            http("request_863")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_864")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_865")
			.get("/assets/images/icon-print.png")
			.headers(headers_46),
            http("request_866")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_867")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_868")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_869")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_870")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_871")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=978128142&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Factions-to-close-the-gap&ul=en-us&de=UTF-8&dt=Actions%20to%20close%20the%20gender%20pay%20gap%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=&gjid=&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_slc=1&z=1013552464")
			.headers(headers_101)))
		.pause(34)
		.exec(http("Visit actions to close the gap - effective actions page")
			.get("/actions-to-close-the-gap/effective-actions")
			.headers(headers_65)
			.resources(http("request_873")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_874")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_875")
			.get("/assets/images/icon-print.png")
			.headers(headers_46),
            http("request_876")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_877")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_878")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_879")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_880")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_881")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=646531301&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Factions-to-close-the-gap%2Feffective-actions&ul=en-us&de=UTF-8&dt=Actions%20to%20close%20the%20gender%20pay%20gap%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=1128677045&gjid=463800317&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=178851368")
			.headers(headers_101)))
		.pause(105)
		.exec(http("Visit actions to close the gap - promising actions page")
			.get("/actions-to-close-the-gap/promising-actions")
			.headers(headers_65)
			.resources(http("request_883")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_884")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_885")
			.get("/assets/images/icon-print.png")
			.headers(headers_46),
            http("request_886")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_887")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_888")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_889")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_890")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_891")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1016405569&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Factions-to-close-the-gap%2Fpromising-actions&ul=en-us&de=UTF-8&dt=Actions%20to%20close%20the%20gender%20pay%20gap%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=844763344&gjid=1111646617&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=839744175")
			.headers(headers_101)))
		.pause(91)
		.exec(http("Visit actions to close the gap - actions with mixed results page")
			.get("/actions-to-close-the-gap/actions-with-mixed-results")
			.headers(headers_65)
			.resources(http("request_893")
			.get("/compiled/app-3382d3058147f5528a18b6339793711fead49fa31e58c9042fa39fbd20992649.css")
			.headers(headers_1),
            http("request_894")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_895")
			.get("/assets/images/icon-print.png")
			.headers(headers_46),
            http("request_896")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_897")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_898")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_899")
			.get("/compiled/app-5dc40c502ce77e74294630b4c91069c44559f4c6584118d752d3804b4dd5c2bd.js")
			.headers(headers_22),
            http("request_900")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_901")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=1474511044&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Factions-to-close-the-gap%2Factions-with-mixed-results&ul=en-us&de=UTF-8&dt=Actions%20to%20close%20the%20gender%20pay%20gap%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=1526827499&gjid=423832363&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=1244165120")
			.headers(headers_101)))
		.pause(70)
		.exec(http("Log out")
			.get("/logout")
			.headers(headers_65)
			.resources(http("request_903")
			.get("/assets/stylesheets/accessible-autocomplete.min.css")
			.headers(headers_1),
            http("request_904")
			.get("/assets/images/govuk-crest-2x.png")
			.headers(headers_46),
            http("request_905")
			.get("/assets/fonts/light-94a07e06a1-v2.woff2")
			.headers(headers_63),
            http("request_906")
			.get("/assets/fonts/bold-b542beb274-v2.woff2")
			.headers(headers_63),
            http("request_907")
			.get(uri2 + "/analytics.js")
			.headers(headers_100),
            http("request_908")
			.post(uri2 + "/j/collect?v=1&_v=j87&a=480110088&t=pageview&_s=1&dl=https%3A%2F%2Flocalhost%2Flogged-out&ul=en-us&de=UTF-8&dt=Signed%20out%20-%20Gender%20pay%20gap%20service&sd=24-bit&sr=1536x864&vp=830x696&je=0&_u=AACAAEABAAAAAC~&jid=1125510708&gjid=579174906&cid=439125182.1606813099&tid=UA-93591336-1&_gid=1587252217.1606813099&_r=1&_slc=1&z=423432145")
			.headers(headers_101)))

	val scn = scenario("LoadTestSimulation202021").exec(
		chain_0, chain_1)

	setUp(scn.inject(
		constantUsersPerSec(1) during (15 seconds)
	)).protocols(httpProtocol)
}