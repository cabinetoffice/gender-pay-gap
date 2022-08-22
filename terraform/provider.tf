// declarations for the providers being used

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.16"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.3.2"
    }
  }

  backend "s3" {
    bucket               = "gender-pay-gap-terraform-state-bucket"
    key                  = "terraform.tfstate"
    dynamodb_table       = "gender-pay-gap-tf-locks"
    region               = "eu-west-2"
    encrypt              = true
    workspace_key_prefix = "gpg"
  }

  experiments = [module_variable_optional_attrs]
}

provider "aws" {
  region = var.aws_region
}