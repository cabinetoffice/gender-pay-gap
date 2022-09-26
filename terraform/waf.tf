resource "aws_wafv2_regex_pattern_set" "ehrc_protected_request_address" {
  name        = "ehrc-rotected-request-address"
  description = "Regex of the endpoint used by ehrc." // Of the form .../download?p=filename
  scope       = "REGIONAL"

  regular_expression {
    regex_string = "^/download(/)?[?]p=(.*)$"
  }
}

resource "aws_wafv2_ip_set" "ehrc_whitelisted_ips" {
  name               = "ehrc-whitelisted-ips"
  description        = "EHRC whitelisted IPs. Only these IPs can access the protected endpoint." // Of the form .../download?p=filename
  scope              = "REGIONAL"
  ip_address_version = "IPV4"
  addresses          = [for ip in split("\n", file("EHRCDownload-IP-Whitelist.txt")) : trimspace(ip)]
}

resource "aws_wafv2_ip_set" "blacklisted_ips" {
  name               = "blacklisted-ips"
  description        = "Blacklisted IPs. These IPs cannot connect to the website."
  scope              = "REGIONAL"
  ip_address_version = "IPV4"
  addresses          = [for ip in split("\n", file("GPG-IP-Denylist.txt")) : trimspace(ip)]
}

resource "aws_wafv2_web_acl" "ehrc" {
  name        = "gpg-acl"
  scope       = "REGIONAL"
  description = "Access control list for the gpg website. Used for securing the EHRC endpoint and limiting bot traffic."

  default_action {
    allow {}
  }

  rule {
    name     = "blacklist"
    priority = 0

    action {
      block {}
    }

    statement {
      ip_set_reference_statement {
        arn = aws_wafv2_ip_set.blacklisted_ips.arn
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = false
      metric_name                = "blacklist-metric"
      sampled_requests_enabled   = false
    }
  }

  rule {
    name     = "rate-limit"
    priority = 1

    action {
      block {}
    }

    statement {
      rate_based_statement {
        limit = 100
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = false
      metric_name                = "rate-limit-metric"
      sampled_requests_enabled   = false
    }
  }

  rule {
    name     = "ehrc-whitelist"
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
            arn = aws_wafv2_regex_pattern_set.ehrc_protected_request_address.arn
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
                arn = aws_wafv2_ip_set.ehrc_whitelisted_ips.arn
              }
            }
          }
        }
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = false
      metric_name                = "ehrc-whitelist-metric"
      sampled_requests_enabled   = false
    }
  }

  visibility_config {
    cloudwatch_metrics_enabled = false
    metric_name                = "alc-metric"
    sampled_requests_enabled   = false
  }
}