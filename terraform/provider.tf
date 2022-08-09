// declarations for the providers being used

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.16"
    }

    random = {
      source = "hashicorp/random"
      version = "3.3.2"
    }
    
  }
  
  // back end config is stored in the back end config files for each environment
  backend "s3" {}
    
  }

locals {
  infra_env = terraform.workspace == "default"  ? "dev" : terraform.workspace
}

provider "aws" {
  # Configuration options
  region = var.aws_region
}

provider "random" {
  # Configuration options
}