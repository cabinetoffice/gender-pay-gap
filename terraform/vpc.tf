module "vpc" {
  // Keeping the 'registry.terraform.io/' prefix so Rider can find the module for autocompletion purposes
  source  = "registry.terraform.io/terraform-aws-modules/vpc/aws"
  version = "3.14.2"

  name = join("-", ["gpg-application-vpc", var.env])
  cidr = "10.0.0.0/16"

  azs                 = slice(data.aws_availability_zones.available.zone_ids, 0, 2)
  public_subnets      = [cidrsubnet("10.0.0.0/16", 4, 0), cidrsubnet("10.0.0.0/16", 4, 1)]
  database_subnets    = [cidrsubnet("10.0.0.0/16", 4, 2), cidrsubnet("10.0.0.0/16", 4, 3)]
  elasticache_subnets = [cidrsubnet("10.0.0.0/16", 4, 4), cidrsubnet("10.0.0.0/16", 4, 5)]

  enable_ipv6                     = true
  assign_ipv6_address_on_creation = true

  database_subnet_assign_ipv6_address_on_creation = false

  public_subnet_ipv6_prefixes   = [0, 1]
  database_subnet_ipv6_prefixes = [2, 3]

  enable_dns_hostnames = true
  enable_dns_support   = true
  
  enable_nat_gateway                     = true
  single_nat_gateway                     = true

  create_database_subnet_group           = true
  create_database_subnet_route_table     = true
  create_database_nat_gateway_route      = true
  create_database_internet_gateway_route = false
  create_elasticache_subnet_group = true

  database_subnet_group_name = join("-", ["gpg-db-subnet-group", var.env])
  database_subnet_group_tags = {
    Name = join("-", ["gpg-db-subnet-group", var.env])
  }

  elasticache_subnet_group_name = join("-", ["gpg-db-elasticache-group", var.env])
  elasticache_subnet_group_tags = {
    Name = join("-", ["gpg-elasticache-subnet-group", var.env])
  }

  public_subnet_tags = {
    Name = join("-", ["gpg-public", var.env])
  }

  private_subnet_tags = {
    Name = join("-", ["gpg-private", var.env])
  }

  vpc_tags = {
    Name = join("-", ["vpc-gpg-application", var.env])
  }
}