locals {
  cloudfront_origin_id = "${local.env_prefix}-load-balancer-origin"
}

resource "aws_cloudfront_distribution" "gpg_distribution" {
  provider = aws.us-east-1

  origin {
    domain_name = data.aws_lb.load_balancer.dns_name
    origin_id   = local.cloudfront_origin_id

    custom_header {
      name  = "X-Custom-Header"
      value = random_integer.load_balancer_custom_header.id
    }
    custom_origin_config {
      http_port              = "80"
      https_port             = "443"
      origin_protocol_policy = "https-only"
      origin_ssl_protocols   = ["TLSv1.2"]
    }

    origin_shield {
      enabled              = true
      origin_shield_region = "eu-west-2"
    }
  }

  enabled         = true
  is_ipv6_enabled = true
  web_acl_id      = aws_wafv2_web_acl.ehrc.arn

  aliases = [var.cloudfront_alternate_domain_name]

  default_cache_behavior {
    allowed_methods  = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods   = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = local.cloudfront_origin_id
    cache_policy_id  = aws_cloudfront_cache_policy.gpg_default.id

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 3600
    max_ttl                = 86400
  }

  # Cache behaviour with precedent 0
  ordered_cache_behavior {
    allowed_methods  = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods   = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = local.cloudfront_origin_id
    cache_policy_id  = aws_cloudfront_cache_policy.static_assets_caching.id
    
    viewer_protocol_policy = "redirect-to-https"
    default_ttl = 604800  // caches static assets for 1 week as standard
    max_ttl     = 864000
    min_ttl     = 1
    path_pattern           = "public/*"
  }

  # Cache behaviour with precedent 1
  ordered_cache_behavior {
    allowed_methods  = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods   = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = local.cloudfront_origin_id
    cache_policy_id  = aws_cloudfront_cache_policy.static_assets_caching.id
    
    viewer_protocol_policy = "redirect-to-https"
    default_ttl = 604800  // caches static assets for 1 week as standard
    max_ttl     = 864000
    min_ttl     = 1
    path_pattern           = "assets/*"
  }

  # Cache behaviour with precedent 2
  ordered_cache_behavior {
    allowed_methods  = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods   = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = local.cloudfront_origin_id
    cache_policy_id  = aws_cloudfront_cache_policy.static_assets_caching.id
    
    viewer_protocol_policy = "redirect-to-https"
    default_ttl = 604800  // caches static assets for 1 week as standard
    max_ttl     = 864000
    min_ttl     = 1
    path_pattern           = "compiled/*"
  }

  # Cache behaviour with precedent 3
  ordered_cache_behavior {
    allowed_methods  = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods   = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = local.cloudfront_origin_id
    
    cache_policy_id  = aws_cloudfront_cache_policy.static_assets_caching.id
    viewer_protocol_policy = "redirect-to-https"
    default_ttl = 604800  // caches static assets for 1 week as standard
    max_ttl     = 864000
    min_ttl     = 1
    path_pattern           = "img/*"
  }

  price_class = "PriceClass_200"

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    acm_certificate_arn            = var.CLOUDFRONT_ACM_CERT_ARN
    cloudfront_default_certificate = true
    ssl_support_method             = "sni-only"
  }

  logging_config {
    include_cookies = true
    bucket          = data.aws_s3_bucket.resource_logs_bucket.bucket_domain_name
    prefix          = local.env_prefix
  }
  
  depends_on = [data.aws_s3_bucket.resource_logs_bucket]

  retain_on_delete = true // QQ remove when application is live
}

resource "aws_cloudfront_cache_policy" "gpg_default" {
  name        = "${local.env_prefix}-default"
  default_ttl = 84600
  max_ttl     = 3156000
  min_ttl     = 1
  parameters_in_cache_key_and_forwarded_to_origin {
    cookies_config {
      cookie_behavior = "all"
    }
    headers_config {
      header_behavior = "whitelist"
      headers {
        items = ["Authorization", "Host", "Origin", "Referer", "User-Agent", "X-Forwarded-For"]
      }
    }
    query_strings_config {
      query_string_behavior = "all"
    }

    enable_accept_encoding_brotli = true
    enable_accept_encoding_gzip   = true
  }
}

resource "aws_cloudfront_cache_policy" "static_assets_caching" {
  name        = "${local.env_prefix}-static-assets-caching"
  default_ttl = 604800  // caches static assets for 1 week as standard
  max_ttl     = 864000
  min_ttl     = 1
  parameters_in_cache_key_and_forwarded_to_origin {
    cookies_config {
      cookie_behavior = "all"
    }
    headers_config {
      header_behavior = "whitelist"
      headers {
        items = ["Authorization", "Host", "Origin", "Referer", "User-Agent", "X-Forwarded-For"]
      }
    }
    query_strings_config {
      query_string_behavior = "all"
    }

    enable_accept_encoding_brotli = true
    enable_accept_encoding_gzip   = true
  }
}

// Contains resource logs that are not automatically exported to cloudwatch
data "aws_s3_bucket" "resource_logs_bucket" {
  bucket = "${local.account_prefix}-resource-logs-bucket"
}

resource "random_integer" "load_balancer_custom_header" {
  min = 1
  max = 50000
  keepers = {
    # Generate a new integer each time we switch to a new load balancer ARN
    load_balancer_arn = data.aws_lb.load_balancer.arn
  }
}