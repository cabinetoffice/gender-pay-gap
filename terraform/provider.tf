
terraform {
  required_version = ">= 1.10.3"

  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "~> 5.82"
    }
  }

  backend "s3" {}
}

provider "aws" {
  region = "eu-west-2" // no alias is provided so will be used as default

  default_tags {
    tags = {
      Service = var.service_name
      Environment = var.environment
      ManagedByTerraform = var.github_url
    }
  }
}

provider "aws" { // This us-east-1 provider is needed for CloudFront (for some reason!)
  region = "us-east-1"
  alias = "us-east-1"

  default_tags {
    tags = {
      Service = var.service_name
      Environment = var.environment
      ManagedByTerraform = var.github_url
    }
  }
}
