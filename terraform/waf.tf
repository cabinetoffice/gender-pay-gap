resource "aws_wafv2_regex_pattern_set" "ehrc_protected_request_address-gpg" {
  provider    = aws.us-east-1
  name        = "ehrc-protected-request-address-${var.env}"
  description = "Regex of the endpoint used by ehrc." // Of the form .../download?p=filename
  scope       = "CLOUDFRONT"

  regular_expression {
    regex_string = "^/download(/)?[?]p=(.*)$"
  }
}

resource "aws_wafv2_regex_pattern_set" "ehrc_protected_request_address" {
  provider    = aws.us-east-1
  name        = "ehrc-rotected-request-address"
  description = "Regex of the endpoint used by ehrc." // Of the form .../download?p=filename
  scope       = "CLOUDFRONT"

  regular_expression {
    regex_string = "^/download(/)?[?]p=(.*)$"
  }
}

resource "aws_wafv2_ip_set" "ehrc_allowlisted_ips" {
  provider           = aws.us-east-1
  name               = "ehrc-allowlisted-ips-${var.env}"
  description        = "EHRC allowlisted IPs. Only these IPs can access the protected endpoint." // Of the form .../download?p=filename
  scope              = "CLOUDFRONT"
  ip_address_version = "IPV4"
  addresses          = [for ip in split("\n", file("EHRCDownload-IP-Whitelist.txt")) : trimspace(ip)]
}

resource "aws_wafv2_ip_set" "denylisted_ips" {
  provider           = aws.us-east-1
  name               = "denylisted-ips-${var.env}"
  description        = "Denylisted IPs. These IPs cannot connect to the website."
  scope              = "CLOUDFRONT"
  ip_address_version = "IPV4"
  addresses          = [for ip in split("\n", file("GPG-IP-Denylist.txt")) : trimspace(ip)]
}

resource "aws_wafv2_web_acl" "ehrc" {
  provider    = aws.us-east-1
  name        = "gpg-acl-${var.env}"
  scope       = "CLOUDFRONT"
  description = "Access control list for the gpg website. Used for securing the EHRC endpoint and limiting bot traffic."

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