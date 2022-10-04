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

#region Relational database configuration 

variable "rds_config_allocated_storage" {
  type        = number
  default     = 100
  description = "Number of GB available to store in the database"
}

variable "rds_config_engine" {
  type        = string
  default     = "postgres"
  description = "The database engine e.g. postgres"
}

variable "rds_config_engine_version" {
  type        = string
  default     = "14"
  description = "The version of the database engine"
}

variable "rds_config_instance_class" {
  type        = string
  description = "The class type of the database e.g. db.t3.small"
}

variable "rds_config_identifier" {
  type        = string
  description = "Database id"
}

variable "rds_config_db_name" {
  type        = string
  description = "Database name"
}

variable "rds_config_port" {
  type        = number
  default     = 5432
  description = "The port the database accepts connections to"
}

variable "rds_config_backup_retention_period" {
  type        = number
  default     = 30
  description = "Number of days backups are kept"
}

variable "rds_config_backup_window" {
  type        = string
  default     = "04:00-05:00"
  description = "Timespan when database backups are performed"
}

variable "rds_config_storage_encrypted" {
  type        = bool
  default     = true
  description = "Specifies if the database is encrypted"
}

variable "rds_config_publicly_accessible" {
  type        = bool
  default     = false
  description = "Specifies if the database is publicly accessible"
}

variable "rds_config_allow_major_version_upgrade" {
  type        = bool
  default     = false
  description = "Specifies if the database can update major versions e.g. 11 -> 12"
}

variable "rds_config_multi_az" {
  type        = bool
  default     = false
  description = "Specifies if the database has multiple availability zones"
}

variable "rds_config_skip_final_snapshot" {
  type        = bool
  default     = false
  description = "Specifies if on deletion a final snapshot should be created"
}

// RDS environment variables
variable "POSTGRES_CONFIG_USERNAME" {
  type        = string
  description = "Database username. Initialized as an environment variable."
}

variable "POSTGRES_CONFIG_PASSWORD" {
  type        = string
  description = "Database password. Initialized as an environment variable."
}

#endregion

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

#region Elastic beanstalk configuration

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

// EB environment variables
variable "ELB_ADMIN_EMAILS" {
  type = string
}

variable "ELB_ASPNETCORE_ENVIRONMENT" {
  type = string
}

variable "ELB_BASIC_AUTH_PASSWORD" {
  type = string
}

variable "ELB_BASIC_AUTH_USERNAME" {
  type = string
}

variable "ELB_COMPANIES_HOUSE_API_KEY" {
  type = string
}

variable "ELB_DATA_MIGRATION_PASSWORD" {
  type = string
}

variable "ELB_DEFAULT_ENCRYPTION_KEY" {
  type = string
}

variable "ELB_DISABLE_SEARCH_CACHE" {
  type = string
}

variable "ELB_EHRC_IP_RANGE" {
  type = string
}

variable "ELB_FEATURE_FLAG_NEW_REPORTING_JOURNEY" {
  type = string
}

variable "ELB_FEATURE_FLAG_PRIVATE_MANUAL_REGISTRATION" {
  type = string
}

variable "ELB_FEATURE_FLAG_SEND_REGISTRATION_REVIEW_EMAILS" {
  type = string
}

variable "ELB_GEO_DISTRIBUTION_LIST" {
  type = string
}

variable "ELB_GOVUK_NOTIFY_API_KEY" {
  type = string
}

variable "ELB_GPG_ANALYSIS_APP_API_PASSWORD" {
  type = string
}

variable "ELB_OBFUSCATION_SEED" {
  type = string
}

variable "ELB_OFFSET_CURRENT_DATE_TIME_FOR_SITE" {
  type = string
}

variable "ELB_REMINDER_EMAIL_DAYS" {
  type = string
}

variable "ELB_REPORTING_START_YEARS_TO_EXCLUDE_FROM_LATE_FLAG_ENFORCEMENT" {
  type = string
}

variable "ELB_REPORTING_START_YEARS_WITH_FURLOUGH_SCHEME" {
  type = string
}

variable "ELB_WEBJOBS_STOPPED" {
  type = string
}

#endregion
#region credidentials

// RDS environment variables
variable "AWS_ACCESS_KEY_ID" {
  type        = string
  description = "AWS access key id. Set as an environment variable."
}

variable "AWS_SECRET_ACCESS_KEY" {
  type        = string
  description = "AWS secret access key. Set as an environment variable."
}

#endregion