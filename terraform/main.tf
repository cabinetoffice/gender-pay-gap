// declarations for the resources to be created 
// this does not include the state S3 bucket as this must be created in order for terraform to init


module "example" {
  source = "registry.terraform.io/terraform-aws-modules/s3-bucket/aws"
  
  bucket = "gpg-bucket-${terraform.workspace}"
  acl    = "private"

  versioning = {
    enabled = true
  }

}

resource "aws_db_subnet_group" "default" {
  name       = "main"
  subnet_ids = ["subnet-0b9845b5d6e0541be", "subnet-04e8deb14870ac711"]

  tags = {
    Name = "My DB subnet group"
  }
}

module "postgresql-rds" {
  source  = "registry.terraform.io/azavea/postgresql-rds/aws"
  version = "3.0.0"
  engine_version = "14.2"
  # insert the 9 required variables here
  alarm_actions = []
  database_identifier = "uniqueidforthisparticulardatabase"
  database_name = "gpgdevdb"
  database_password = "local_gpg_database"
  database_username = "gpg_user"
  database_port = 5432
  insufficient_data_actions = []
  ok_actions = []
  subnet_group = aws_db_subnet_group.default.name
  vpc_id = "vpc-0e9491851e4208470"
  parameter_group = "testparamgroup"
}