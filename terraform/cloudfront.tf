resource "aws_cloudfront_distribution" "gpg-load-balancer-origin" {
  provider            = aws.us-east-1   
  
  origin {
    domain_name = aws_elastic_beanstalk_environment.gpg-elb-environment.endpoint_url
    origin_id   = var.cloudfront_origin_id
    
    origin_shield {
      enabled              = true
      origin_shield_region = "us-east-2"
    }
  }

  enabled             = true
  is_ipv6_enabled     = true
  web_acl_id = aws_wafv2_web_acl.ehrc.id

  aliases = []  // cnames string comma sep list

  default_cache_behavior {
    allowed_methods  = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods   = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = var.cloudfront_origin_id

    forwarded_values { // deprecated fields. resolved instead through cache policy 
      query_string = false

      cookies {
        forward = "none"
      }
    }

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 3600
    max_ttl                = 86400
  }

  # Cache behavior with precedence 0
  ordered_cache_behavior {
    path_pattern     = "/images/*.png"
    allowed_methods  = ["GET", "HEAD", "OPTIONS"]
    cached_methods   = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = var.cloudfront_origin_id

    forwarded_values {
      query_string = false
      headers      = ["Origin"]

      cookies {
        forward = "none"
      }
    }

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 86400
    max_ttl                = 31536000
    compress               = true
  }

  price_class = "PriceClass_200"
  
  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  tags = {
    env = "dev"
  }

  viewer_certificate {
    acm_certificate_arn = var.CLOUDFRONT_ACM_CERT_ARN
    ssl_support_method = "sni-only"
  }
  
}