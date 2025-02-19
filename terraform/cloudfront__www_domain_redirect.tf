
data "aws_cloudfront_cache_policy" "cache_policy__aws_managed_caching_optimised" {
    name = "Managed-CachingOptimized"
}

locals {
  distribution_for_www_domain_redirect__origin_id = "${var.service_name_hyphens}--${var.environment_hyphens}--www-Domain-Redirect-origin"
}

resource "aws_cloudfront_distribution" "distribution__www_domain_redirect" {
  count = (var.create_redirect_from_www_domain) ? 1 : 0  // Only create this CloudFront Distribution if "var.create_redirect_from_www_domain" is true

  // CloudFront distributions have to be created in the us-east-1 region (for some reason!)
  provider = aws.us-east-1

  comment = "${var.service_name_hyphens}--${var.environment_hyphens}--www-domain-redirect"

  origin {
    domain_name = "example.com" // The origin domain doesn't matter because we use a CloudFront Function to redirect all traffic.
                                // We shouldn't use a real domain in case any traffic is forwarded, but example.com is reserved as an unused domain name (https://en.wikipedia.org/wiki/Example.com)
    origin_id = local.distribution_for_www_domain_redirect__origin_id

    custom_origin_config {
      http_port = 80
      https_port = 443
      origin_protocol_policy = "http-only"
      origin_ssl_protocols = ["TLSv1.2"]
    }
  }

  price_class = "PriceClass_100"

  aliases = ["${var.dns_record_www_domain_including_dot}${data.aws_route53_zone.route_53_zone_for_our_domain.name}"]

  viewer_certificate {
    acm_certificate_arn = aws_acm_certificate_validation.certificate_validation_waiter__www_domain_redirect[0].certificate_arn
    cloudfront_default_certificate = false
    minimum_protocol_version = "TLSv1"
    ssl_support_method = "sni-only"
  }

  enabled = true
  is_ipv6_enabled = true

  default_cache_behavior {
    cache_policy_id = data.aws_cloudfront_cache_policy.cache_policy__aws_managed_caching_optimised.id
    allowed_methods = ["GET", "HEAD"]
    cached_methods = ["GET", "HEAD"]
    target_origin_id = local.distribution_for_www_domain_redirect__origin_id
    viewer_protocol_policy = "redirect-to-https"
    compress = true

    function_association {
      event_type = "viewer-request"
      function_arn = aws_cloudfront_function.redirect_www_domain_function[0].arn
    }
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
      locations = []
    }
  }
}

resource "aws_cloudfront_function" "redirect_www_domain_function" {
  count = (var.create_redirect_from_www_domain) ? 1 : 0  // Only create this CloudFront Function if "var.create_redirect_from_www_domain" is true

  name    = "${var.service_name_hyphens}--${var.environment_hyphens}--redirect-www-domain-function"
  runtime = "cloudfront-js-1.0"
  publish = true
  code    = <<EOT
function handler(event) {
    var response = {
        statusCode: 302,
        statusDescription: 'Found',
        headers: {
            "location": { "value": "https://${var.dns_record_subdomain_including_dot}${data.aws_route53_zone.route_53_zone_for_our_domain.name}" + event.request.uri }
        }
    };
    return response;
}
EOT
}
