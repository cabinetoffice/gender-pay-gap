
resource "aws_vpc" "vpc_main" {
  cidr_block = "10.0.0.0/16"
  instance_tenancy = "default"
  
  enable_dns_support = true
  enable_dns_hostnames = true

  tags = {
    Name = "${var.service_name}__${var.environment}__VPC_Main"
  }
}

resource "aws_subnet" "vpc_main__public_subnet_az1" {
  vpc_id     = aws_vpc.vpc_main.id
  cidr_block = "10.0.1.0/24"
  availability_zone = "eu-west-2a"

  tags = {
    Name = "${var.service_name}__${var.environment}__vpc_main__public_subnet_az1"
  }
}

resource "aws_subnet" "vpc_main__public_subnet_az2" {
  vpc_id     = aws_vpc.vpc_main.id
  cidr_block = "10.0.2.0/24"
  availability_zone = "eu-west-2b"
  
  tags = {
    Name = "${var.service_name}__${var.environment}__vpc_main__public_subnet_az2"
  }
}

resource "aws_internet_gateway" "vpc_main__internet_gateway" {
  vpc_id = aws_vpc.vpc_main.id

  tags = {
    Name = "${var.service_name}__${var.environment}__VPC_Main__Internet_Gateway"
  }
}

resource "aws_route_table" "vpc_main__route_table_main" {
  vpc_id = aws_vpc.vpc_main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.vpc_main__internet_gateway.id
  }

  tags = {
    Name = "${var.service_name}__${var.environment}__VPC_Main__Route_Table_Main"
  }
}

resource "aws_main_route_table_association" "vpc_main__main_route_table_association" {
  vpc_id = aws_vpc.vpc_main.id
  route_table_id = aws_route_table.vpc_main__route_table_main.id
}
