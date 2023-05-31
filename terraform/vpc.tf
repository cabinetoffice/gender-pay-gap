// Available availability zones
data "aws_availability_zones" "available" {
  state = "available"
}

module "vpc" {
  // Keeping the 'registry.terraform.io/' prefix so Rider can find the module for autocompletion purposes
  source  = "registry.terraform.io/terraform-aws-modules/vpc/aws"
  version = "3.14.2"

  name = "${local.env_prefix}-application-vpc"
  cidr = "10.0.0.0/16"

  azs                 = slice(data.aws_availability_zones.available.zone_ids, 0, 2)
  public_subnets      = [cidrsubnet("10.0.0.0/16", 4, 0), cidrsubnet("10.0.0.0/16", 4, 1)]
  database_subnets    = [cidrsubnet("10.0.0.0/16", 4, 2), cidrsubnet("10.0.0.0/16", 4, 3)]

  enable_ipv6                     = true
  assign_ipv6_address_on_creation = true

  database_subnet_assign_ipv6_address_on_creation = false

  public_subnet_ipv6_prefixes      = [0, 1]
  database_subnet_ipv6_prefixes    = [2, 3]

  enable_dns_hostnames = true
  enable_dns_support   = true

  enable_nat_gateway = true
  single_nat_gateway = false

  create_database_subnet_group           = true
  create_database_subnet_route_table     = true
  create_database_nat_gateway_route      = true
  create_database_internet_gateway_route = true

  database_subnet_group_name = "${local.env_prefix}-db-subnet-group"
  database_subnet_group_tags = {
    Name = "${local.env_prefix}-db-subnet-group"
  }

  public_subnet_tags = {
    Name = "${local.env_prefix}-public"
  }

  private_subnet_tags = {
    Name = "${local.env_prefix}-private"
  }

  vpc_tags = {
    Name = "${local.env_prefix}-vpc"
  }

  // Logging and monitoring
  enable_flow_log                                 = true
  flow_log_cloudwatch_iam_role_arn                = aws_iam_role.cloudwatch-flow-log.arn
  flow_log_cloudwatch_log_group_retention_in_days = 60
  flow_log_destination_arn                        = aws_cloudwatch_log_group.gpg-flow-log.arn
  flow_log_destination_type                       = "cloud-watch-logs"
  flow_log_per_hour_partition                     = true
}

resource "aws_cloudwatch_log_group" "gpg-flow-log" {
  name = "${local.env_prefix}-flow-logs"
}

resource "aws_iam_role" "cloudwatch-flow-log" {
  name = "${local.env_prefix}-cloudwatch-flow-logs"

  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "",
      "Effect": "Allow",
      "Principal": {
        "Service": "vpc-flow-logs.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "cloudwatch-flow-log" {
  name = "${local.env_prefix}-cloudwatch-iam-role-policy"
  role = aws_iam_role.cloudwatch-flow-log.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents",
        "logs:DescribeLogGroups",
        "logs:DescribeLogStreams"
      ],
      "Effect": "Allow",
      "Resource": "*"
    }
  ]
}
EOF
}