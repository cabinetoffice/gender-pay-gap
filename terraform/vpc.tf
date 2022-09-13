module "vpc" {
  source  = "registry.terraform.io/terraform-aws-modules/vpc/aws"
  version = "3.14.2"

  name = join("-",["gpg-application-vpc",var.env])
  cidr = "10.0.0.0/16"

  azs             = slice(data.aws_availability_zones.available.zone_ids, 0, 2)
  public_subnets  = [cidrsubnet("10.0.0.0/16", 4, 0), cidrsubnet("10.0.0.0/16", 4, 1)]
  database_subnets = [cidrsubnet("10.0.0.0/16", 4, 2), cidrsubnet("10.0.0.0/16", 4, 3)]

  enable_ipv6                     = true
  assign_ipv6_address_on_creation = true

  database_subnet_assign_ipv6_address_on_creation = false

  public_subnet_ipv6_prefixes   = [0, 1]
  database_subnet_ipv6_prefixes = [2, 3]

  enable_dns_hostnames = true
  enable_dns_support = true

  enable_nat_gateway                     = false
  single_nat_gateway                     = true
  create_database_subnet_group           = true
  create_database_subnet_route_table     = true
  create_database_nat_gateway_route      = true
  create_database_internet_gateway_route = false

  database_subnet_group_name = join("-",["gpg-db-subnet-group", var.env])
  database_subnet_group_tags = {
    Name = join("-",["gpg-db-subnet-group", var.env])
  }

  public_subnet_tags = {
    Name = join("-",["gpg-public", var.env])
  }

  private_subnet_tags = {
    Name = join("-",["gpg-private",var.env])
  }

  vpc_tags = {
    Name = join("-",["vpc-gpg-application",var.env])
  }
}

resource "aws_wafv2_regex_pattern_set" "ehrc_protected_request_address" {
  name        = "ehrc-rotected-request-address"
  description = "Regex of the endpoint used by ehrc"
  scope       = "REGIONAL"

  regular_expression {
    regex_string = "^/download(/)?[?]p=(.*)$"
  }
}

resource "aws_wafv2_ip_set" "ehrc_whitelisted_ips" {
  name               = "ehrc-whitelisted-ips"
  description        = "EHRC whitelisted IPs"
  scope              = "REGIONAL"
  ip_address_version = "IPV4"
  addresses          = ["0.0.0.0/0, 10.0.0.0/16"]
}

resource "aws_wafv2_web_acl" "ehrc" {
  name  = "ehrc-access-control-list"
  scope = "REGIONAL"
  
  default_action {
    allow {}
  }
  
  rule {
    name     = "ehrc whitelist"
    priority = 0
    
    action {
      block {}
    }
    
    statement {
      and_statement {
        statement {
          regex_pattern_set_reference_statement {
            arn = aws_wafv2_regex_pattern_set.ehrc_protected_request_address.arn
            text_transformation {
              priority = 0
              type     = "NONE"
            }
          }
        }

        statement {
          ip_set_reference_statement {
            arn = aws_wafv2_ip_set.ehrc_whitelisted_ips.arn
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
    metric_name                = "ehrc-metric"
    sampled_requests_enabled   = false
  }

}