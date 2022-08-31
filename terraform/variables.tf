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

variable "env" {
  description = "The environment file used"
  type        = string
}

variable "postgres_config" {
  type = object({
    allocated_storage           = optional(number)
    engine                      = optional(string)
    engine_version              = optional(string)
    instance_class              = string
    identifier                  = string
    db_name                     = string
    username                    = optional(string)
    password                    = optional(string)
    port                        = optional(number)
    backup_retention_period     = optional(number)
    backup_window               = optional(string)
    storage_encrypted           = optional(bool)
    publicly_accessible         = optional(bool)
    allow_major_version_upgrade = optional(bool)
    multi_az                    = optional(bool)
    skip_final_snapshot         = optional(bool)
    final_snapshot_identifier   = optional(string)
  })
  description = "Contains configuration options for postgres databases"
}

variable "POSTGRES_CONFIG_USERNAME" {
  type = string
  description = "Postgres database username. Initialized as an environment variable."
}

variable "POSTGRES_CONFIG_PASSWORD" {
  type = string
  description = "Postgres database password. Initialized as an environment variable."
}