resource "aws_cloudfront_distribution" "gpg-distribution" {
  provider            = aws.us-east-1   
  
  origin {
    domain_name = aws_elastic_beanstalk_environment.gpg-elb-environment.load_balancers[0]
    origin_id   = var.cloudfront_origin_id
    custom_origin_config {
      http_port              = "80"
      https_port             = "443"
      origin_protocol_policy = "http-only"
      origin_ssl_protocols   = ["TLSv1", "TLSv1.1", "TLSv1.2"]
    }
    
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
      query_string = true

      cookies {
        forward = "all"
      }
    }

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

  tags = {
    env = "dev"
  }

  viewer_certificate {
    acm_certificate_arn = var.CLOUDFRONT_ACM_CERT_ARN
    ssl_support_method = "sni-only"
  }
  
}