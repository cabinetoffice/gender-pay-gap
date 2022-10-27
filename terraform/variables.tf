// all declared input variables

variable "aws_region" {
  description = "The AWS region used for the provider and resources."
  default     = "eu-west-2"
}

variable "env" {
  description = "The environment name"
  type        = string
}

#region credentials 

variable "AWS_ACCESS_KEY_ID" {
  type        = string
  description = "AWS access key id. Set as an environment variable."
}

variable "AWS_SECRET_ACCESS_KEY" {
  type        = string
  description = "AWS secret access key. Set as an environment variable."
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

variable "rds_config_port" {
  type        = number
  default     = 5432
  description = "The port the database accepts connections to"
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

variable "elb_cname_prefix" {
  type        = string
  description = "Optional prefix that sets DNS name of the eb env. Must check prefix availability on AWS console."
}

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

variable "elb_instance_profile" {
  type        = string
  description = "An instance profile name or arn that enables ELB to access temporary security credentials to make AWS API calls."
}

variable "elb_instance_type" {
  type        = string
  description = "The instance type that's used to run the application in the Elastic Beanstalk environment."
}

variable "elb_lb_scheme" {
  type        = string
  description = "Specifies whether the Elastic Beanstalk load balancer is public or internal"
}

variable "ELB_LOAD_BALANCER_SSL_CERTIFICATE_ARNS" {
  type        = string
  description = "The certificate arn for Load Balancer. Passed in as secret in azure devops"
}

variable "elb_load_balancer_type" {
  type        = string
  description = "The type of load balancer for the Elastic Beanstalk environment."
}

variable "elb_solution_stack_name" {
  type        = string
  description = "A solution stack to base Elastic Beanstalk environment off of."
}

variable "elb_ssl_policy" {
  type        = string
  description = "The name of the Elastic Beanstalk's load balancer security policy."
}

variable "elb_tier" {
  type        = string
  description = "Elastic Beanstalk Environment tier. Valid values are Worker or WebServer."
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

#region elasticache config

variable "elasticache_cache_port" {
  type        = number
  description = "The port number on which each of the cache nodes will accept connections. The default port is 6379."
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

variable "cloudfront_logging_prefix" {
  type        = string
  description = "An optional string that prefixes to the access log filenames for this distribution"
}

variable "cloudfront_origin_id" {
  type        = string
  description = "A unique identifier for the origin."
}

#endregion

#region cloudwatch config

variable "cloudwatch_notification_emails" {
  type = string
  description = "A softwire email distribution list that will be notified of alarm breaches"
  default = "Team-GenderPayGap@softwire.com"
}

#endregion