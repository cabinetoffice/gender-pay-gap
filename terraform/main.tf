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

resource "aws_db_instance" "gpg-dev-db" {
  allocated_storage    = 20
  engine               = "postgres"
  engine_version       = "14"
  identifier           =  "gpg-dev-db"
  instance_class       = "db.t3.medium"
  name                 = "gpgdevdb"
  username             = "testuser"
  password             = "testpassword"
  skip_final_snapshot  = true
  publicly_accessible  = true
}