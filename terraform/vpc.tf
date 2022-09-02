module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"
  version = "3.14.2"

  name = "gpg-application-vpc"
  cidr = "10.0.0.0/16"

  azs             = slice(data.aws_availability_zones.available.zone_ids, 0, 2)
  public_subnets  = [cidrsubnet("10.0.0.0/16", 4, 1), cidrsubnet("10.0.0.0/16", 4, 2)]
  database_subnets = [cidrsubnet("10.0.0.0/16", 4, 3), cidrsubnet("10.0.0.0/16", 4, 4)]

/*  enable_ipv6                     = true
  assign_ipv6_address_on_creation = true*/

  enable_dns_hostnames = true
  enable_dns_support = true
  
  enable_nat_gateway                     = false
  single_nat_gateway                     = true
  create_database_subnet_group           = true
  create_database_subnet_route_table     = true
  create_database_nat_gateway_route      = true
  create_database_internet_gateway_route = false
  
  database_subnet_group_name = "gpg-db-subnet-group"
  database_subnet_group_tags = {
    Name = "gpg-db-subnet-group"
  }

  public_subnet_tags = {
    Name = "gpg-public"
  }

  private_subnet_tags = {
    Name = "gpg-private"
  }

  vpc_tags = {
    Name = "vpc-gpg-application"
  }
}