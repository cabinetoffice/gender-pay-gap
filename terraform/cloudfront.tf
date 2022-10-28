resource "aws_cloudfront_distribution" "gpg-distribution" {
  provider = aws.us-east-1

  origin {
    domain_name = data.aws_lb.load-balancer.dns_name
    origin_id   = var.cloudfront_origin_id

    custom_header {
      name  = "X-Custom-Header"
      value = random_integer.load-balancer-custom-header.id
    }
    custom_origin_config {
      http_port              = "80"
      https_port             = "443"
      origin_protocol_policy = "https-only"
      origin_ssl_protocols   = ["TLSv1.2"]
    }

    origin_shield {
      enabled              = true
      origin_shield_region = "us-east-2"
    }
  }

  enabled         = true
  is_ipv6_enabled = true
  web_acl_id      = aws_wafv2_web_acl.ehrc.arn

  aliases = [var.cloudfront_alternate_domain_name]

  default_cache_behavior {
    allowed_methods  = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods   = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = var.cloudfront_origin_id
    cache_policy_id  = aws_cloudfront_cache_policy.authorisation.id

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 3600
    max_ttl                = 86400
  }

  price_class = "PriceClass_200"

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    acm_certificate_arn            = var.CLOUDFRONT_ACM_CERT_ARN
    cloudfront_default_certificate = false
    ssl_support_method             = "sni-only"
  }

  logging_config {
    include_cookies = false
    bucket          = aws_s3_bucket.resource-logs-bucket.bucket_domain_name
    prefix          = var.cloudfront_logging_prefix
  }
  depends_on = [aws_s3_bucket.resource-logs-bucket]
}

resource "aws_cloudfront_cache_policy" "authorisation" {
  name        = "authorisation-policy-${var.env}"
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
        items = ["Authorization", "Host", "Origin", "Referer", "User-Agent"]
      }
    }
    query_strings_config {
      query_string_behavior = "all"
    }

    enable_accept_encoding_brotli = true
    enable_accept_encoding_gzip   = true
  }
}

resource "aws_s3_bucket" "resource-logs-bucket" {
  bucket = "resource-log-bucket-${var.env}"
}

resource "random_integer" "load-balancer-custom-header" {
  min = 1
  max = 50000
  keepers = {
    # Generate a new integer each time we switch to a new load balancer ARN
    load_balancer_arn = data.aws_lb.load-balancer.arn
  }
}