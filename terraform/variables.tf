// all declared input variables

variable "aws_region" {
  description = "The AWS region used for the provider and resources."
  default     = "eu-west-2"
}

variable "example_s3_versioning_enabled" {
  type        = bool
  description = "A bool to enable versioning on the test S3 bucket"
  default     = true
}

variable "gpg_application" {}

variable "solution_stack_name" {
  type = string
}

variable "tier" {
  type = string
}

variable "env" {
  description = "The environment name"
  type        = string
}

variable "instance_type" {}

variable "elb_instance_min_size" {}

variable "elb_instance_max_size" {}

variable "elb_instance_profile" {}

variable "elb_load_balancer_type" {}

variable "elb_scheme" {}

variable "elb_matcher_http_code" {}

variable "elb_health_reporting_system_type" {
  default = "enhanced"
}

variable "cache_port" {
}