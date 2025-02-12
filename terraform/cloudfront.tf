
data "aws_cloudfront_cache_policy" "cloudfront_cache_policy__managed_caching_disabled" {
  name = "Managed-CachingDisabled"
}

data "aws_cloudfront_origin_request_policy" "cloudfront_origin_request_policy__managed_all_viewer" {
  name = "Managed-AllViewer"
}

resource "aws_cloudfront_cache_policy" "cloudfront_cache_policy__cache_for_1_day" {
  name = "${var.service_name_hyphens}--${var.environment_hyphens}-Cache-Policy--Cache-For-1-Day"
  min_ttl = 300
  default_ttl = 86400
  max_ttl = 86400

  parameters_in_cache_key_and_forwarded_to_origin {
    cookies_config {
      cookie_behavior = "none"
    }
    headers_config {
      header_behavior = "none"
    }
    query_strings_config {
      query_string_behavior = "none"
    }
    
    enable_accept_encoding_brotli = true
    enable_accept_encoding_gzip = true
  }
}

locals {
  distribution__origin_id = "${var.service_name_hyphens}--${var.environment_hyphens}--origin"
}

resource "aws_cloudfront_distribution" "distribution" {
  // CloudFront distributions have to be created in the us-east-1 region (for some reason!)
  provider = aws.us-east-1

  comment = "${var.service_name_hyphens}--${var.environment_hyphens}"

  origin {
    domain_name = aws_elastic_beanstalk_environment.main_app_elastic_beanstalk_environment.cname
    origin_id = local.distribution__origin_id

    custom_origin_config {
      http_port = 80
      https_port = 443
      origin_protocol_policy = "http-only"
      origin_ssl_protocols = ["TLSv1.2"]
    }
    
    custom_header {
      name  = "X-Forwarded-Proto"
      value = "https"
    }
  }

  price_class = "PriceClass_100"

  # aliases = ["${var.dns_record_subdomain_including_dot}${data.aws_route53_zone.route_53_zone_for_our_domain.name}"]
  
  viewer_certificate {
    # acm_certificate_arn = aws_acm_certificate_validation.certificate_validation_waiter.certificate_arn
    # cloudfront_default_certificate = false
    # minimum_protocol_version = "TLSv1"
    # ssl_support_method = "sni-only"
    cloudfront_default_certificate = true
  }

  enabled = true
  is_ipv6_enabled = true

  default_cache_behavior {
    allowed_methods = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = local.distribution__origin_id
    cache_policy_id = data.aws_cloudfront_cache_policy.cloudfront_cache_policy__managed_caching_disabled.id
    origin_request_policy_id = data.aws_cloudfront_origin_request_policy.cloudfront_origin_request_policy__managed_all_viewer.id
    viewer_protocol_policy = "redirect-to-https"
    compress = true
  }
  
  ordered_cache_behavior {
    path_pattern = "compiled/*"
    allowed_methods = ["GET", "HEAD", "OPTIONS"]
    cached_methods = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = local.distribution__origin_id
    viewer_protocol_policy = "redirect-to-https"
    cache_policy_id = aws_cloudfront_cache_policy.cloudfront_cache_policy__cache_for_1_day.id
  }
  
  ordered_cache_behavior {
    path_pattern = "assets/*"
    allowed_methods = ["GET", "HEAD", "OPTIONS"]
    cached_methods = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = local.distribution__origin_id
    viewer_protocol_policy = "redirect-to-https"
    cache_policy_id = aws_cloudfront_cache_policy.cloudfront_cache_policy__cache_for_1_day.id
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
      locations = []
    }
  }
}
