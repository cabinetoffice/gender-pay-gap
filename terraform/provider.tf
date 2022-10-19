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

    null = {
      source  = "hashicorp/null"
      version = "3.1.1"
    }

  }

  backend "s3" {}
}

provider "aws" {
  region = var.aws_region  // no alias is provided so will be used as default
}

provider "aws" {
  region = "us-east-1"
  alias  = "us-east-1"
}