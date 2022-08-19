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

variable "postgres_config" {
  type = object({
    allocated_storage = number
    engine = string
    engine_version = string
    instance_class = string
    identifier                  = string
    db_name                     = string
    username                    = string
    password                    = string
    port                        = number
    backup_retention_period     = number
    backup_window               = string
    storage_encrypted           = bool
    publicly_accessible         = bool
    allow_major_version_upgrade = bool
  })
  description = "Contains configuration options for postgres databases"
}