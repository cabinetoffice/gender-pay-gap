resource "aws_wafv2_regex_pattern_set" "ehrc_protected_request_address-gpg" {
  provider    = aws.us-east-1
  name        = "${local.env_prefix}-ehrc-protected-request-address"
  description = "Regex of the endpoint used by ehrc." // Of the form .../download?p=filename
  scope       = "CLOUDFRONT"

  regular_expression {
    regex_string = "^/download(/)?[?]p=(.*)$"
  }

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_wafv2_ip_set" "ehrc_allowlisted_ips" {
  provider           = aws.us-east-1
  name               = "${local.env_prefix}-ehrc-allowlisted-ips"
  description        = "EHRC allowlisted IPs. Only these IPs can access the protected endpoint." // Of the form .../download?p=filename
  scope              = "CLOUDFRONT"
  ip_address_version = "IPV4"
  addresses          = [for ip in split("\n", file("EHRCDownload-IP-Whitelist.txt")) : trimspace(ip)]
}

resource "aws_wafv2_ip_set" "denylisted_ips" {
  provider           = aws.us-east-1
  name               = "${local.env_prefix}-denylisted-ips"
  description        = "Denylisted IPs. These IPs cannot connect to the website."
  scope              = "CLOUDFRONT"
  ip_address_version = "IPV4"
  addresses          = [for ip in split("\n", file("GPG-IP-Denylist.txt")) : trimspace(ip)]
}

resource "aws_wafv2_web_acl" "ehrc" {
  provider    = aws.us-east-1
  name        = "${local.env_prefix}gpg-acl"
  scope       = "CLOUDFRONT"
  description = "Access control list for the gpg website. Used for securing the EHRC endpoint and limiting bot traffic."

  lifecycle {
    create_before_destroy = true
  }

  default_action {
    allow {}
  }

  rule {
    name     = "denylist"
    priority = 0

    action {
      block {}
    }

    statement {
      ip_set_reference_statement {
        arn = aws_wafv2_ip_set.denylisted_ips.arn
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = false
      metric_name                = "denylist-metric"
      sampled_requests_enabled   = false
    }
  }

  rule {
    name     = "ehrc-allowlist"
    priority = 2

    action {
      block {}
    }

    statement {
      and_statement {
        statement {
          regex_pattern_set_reference_statement {
            field_to_match {
              uri_path {}
            }
            arn = aws_wafv2_regex_pattern_set.ehrc_protected_request_address-gpg.arn
            text_transformation {
              priority = 0
              type     = "NONE"
            }
          }
        }

        statement {
          not_statement {
            statement {
              ip_set_reference_statement {
                arn = aws_wafv2_ip_set.ehrc_allowlisted_ips.arn
              }
            }
          }
        }
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = false
      metric_name                = "ehrc-allowlist-metric"
      sampled_requests_enabled   = false
    }
  }

  visibility_config {
    cloudwatch_metrics_enabled = false
    metric_name                = "alc-metric"
    sampled_requests_enabled   = false
  }
}

resource "aws_wafv2_web_acl_logging_configuration" "waf-logging-config" {
  provider                = aws.us-east-1
  log_destination_configs = [aws_cloudwatch_log_group.waf-log-group.arn]
  resource_arn            = aws_wafv2_web_acl.ehrc.arn
  redacted_fields {
    single_header {
      name = "user-agent"
    }
  }
}

resource "aws_cloudwatch_log_group" "waf-log-group" {
  provider = aws.us-east-1
  name     = "aws-waf-logs-${local.env_prefix}" //the prefix "aws-waf-logs-" is required
}