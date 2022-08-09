// declarations for the resources to be created 
// this does not include the state S3 bucket as this must be created in order for terraform to init


module "s3_bucket" {
  source = "terraform-aws-modules/s3-bucket/aws"

  
  bucket = "gpg-bucket-${terraform.workspace}"
  acl    = "private"

  versioning = {
    enabled = true
  }

}