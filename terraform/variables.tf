// all declared input variables

variable "aws_region" {
  type        = string
  description = "The AWS region used for the provider and resources."
  default     = "eu-west-2"
}

variable "env" {
  type        = string
  description = "The environment name."
}

variable "account" {
  type        = string
  description = "The AWS Cabinet Office account the environment is created in."
}

#region credentials 

variable "AWS_ACCESS_KEY_ID" {
  type        = string
  description = "AWS access key id for terraform programmatic access. Set as an environment variable."
}

variable "AWS_SECRET_ACCESS_KEY" {
  type        = string
  description = "AWS secret access key id for terraform programmatic access. Set as an environment variable."
}
#endregion

#region Relational database configuration

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

variable "rds_config_multi_az" {
  type        = bool
  default     = false
  description = "Specifies if the database has multiple availability zones"
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

#region Elastic beanstalk configuration

variable "elb_deployment_policy" {
  type        = string
  description = "The deployment policy for Elastic Beanstalk application version deployments."
}

variable "elb_instance_max_size" {
  type        = number
  description = "The maximum number of instances in Elastic Beanstalk Auto Scaling group."
}

variable "elb_instance_min_size" {
  type        = number
  description = "The minimum number of instances in Elastic Beanstalk Auto Scaling group."
}

variable "elb_instance_type" {
  type        = string
  description = "The instance type that's used to run the application in the Elastic Beanstalk environment."
}

variable "ELB_LOAD_BALANCER_SSL_CERTIFICATE_ARN" {
  type        = string
  description = "The certificate arn for Load Balancer. Passed in as secret in azure devops"
}

// Elastic Beanstalk environment variables
// These are set in azure devops
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

variable "ELB_ENABLE_CONSOLE_LOGGING" {
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

#region cloudfront config

variable "CLOUDFRONT_ACM_CERT_ARN" {
  type        = string
  description = "The ARN of the ACM certificate used with this distribution. It must be in the us-east-1 region."
}

variable "cloudfront_alternate_domain_name" {
  type        = string
  description = "Any additional CNAMEs or Alias records, if any, for this distribution."
}

#endregion

#region cloudwatch config

variable "CLOUDWATCH_NOTIFICATION_EMAILS" {
  type        = string
  description = "An email distribution list to be notified of alarm breaches. Pass in as environment variable."
  default     = "ladun.omideyi@softwire.com"
}

#endregion