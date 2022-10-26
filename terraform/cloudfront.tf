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

  aliases = [var.route_53_domain]

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
        items = ["Authorization", "Host"]
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

// Alarms

resource "aws_cloudwatch_metric_alarm" "no_healthy_hosts" {
  alarm_name          = "healthy-host-alarm-${var.env}"
  metric_name         = "HealthyHostCount"
  namespace           = "AWS/ApplicationELB"
  comparison_operator = "LessThanOrEqualToThreshold"
  statistic           = "Minimum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  alarm_description   = "[${var.env} - The Performance Platform has no healthy hosts. Refer to the runbook."
  alarm_actions       = [aws_sns_topic.cloudwatch_alarm.arn]
  ok_actions          = [aws_sns_topic.cloudwatch_alarm.arn]
  dimensions = {
    AutoScalingGroupName = data.aws_autoscaling_group.elb_autoscaling.name
  }
}

resource "aws_cloudwatch_metric_alarm" "http_errors" {
  alarm_name          = "${var.env}-http-errors"
  metric_name         = "HTTPCode_Target_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  comparison_operator = "GreaterThanThreshold"
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  alarm_description   = "[${var.env} - The Performance Platform has HTTP 5xx errors. Refer to the runbook."
  alarm_actions       = [aws_sns_topic.cloudwatch_alarm.arn]
  ok_actions          = [aws_sns_topic.cloudwatch_alarm.arn]
  treat_missing_data  = "notBreaching"
  dimensions = {
    LoadBalancer = aws_elastic_beanstalk_environment.gpg-elb-environment.load_balancers[0]
  }
}

# No instances should fail the health checks unless they failed to boot.
# This usually means a release failed and will need manual intervention.
resource "aws_cloudwatch_metric_alarm" "unhealthy_hosts" {
  alarm_name          = "-unhealthy-hosts-${var.env}"
  metric_name         = "UnHealthyHostCount"
  namespace           = "AWS/ApplicationELB"
  comparison_operator = "GreaterThanThreshold"
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  alarm_description   = "[${var.env} - The Performance Platform has unhealthy hosts. Refer to the runbook."
  alarm_actions       = [aws_sns_topic.cloudwatch_alarm.arn]
  ok_actions          = [aws_sns_topic.cloudwatch_alarm.arn]
  treat_missing_data  = "notBreaching"
  dimensions = {
    AutoScalingGroupName = data.aws_autoscaling_group.elb_autoscaling.name
  }
}

resource "aws_sns_topic" "cloudwatch_alarm" {
  name = "cloudwatch-${var.env}"
}