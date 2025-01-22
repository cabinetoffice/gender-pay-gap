
variable "service_name" {
  type = string
  description = "The short name of the service."
  default = "gpg"
}
variable "service_name_hyphens" {
  type = string
  description = "The short name of the service (using hyphen-style)."
  default = "gpg"
}

variable "environment" {
  type = string
  description = "The environment name."
}
variable "environment_hyphens" {
  type = string
  description = "The environment name (using hyphen-style)."
}

variable "github_url" {
  type = string
  description = "The URL to the GitHub repo (we add this as a Tag to all resources)"
}

variable "create_dns_record" {
  type = bool
  description = "Should terraform create a Route53 alias record for the (sub)domain"
}
variable "dns_record_subdomain_including_dot" {
  type = string
  description = "The subdomain (including dot - e.g. 'dev.' or just '' for production) for the Route53 alias record"
}

// SECRETS
// These variables are set in GitHub Actions environment-specific secrets
// Most of these are passed to the application via Elastic Beanstalk environment variables
variable "POSTGRES_PASSWORD" {
  type = string
  sensitive = true
}

variable "DEFAULT_ENCRYPTION_KEY" {
  type = string
  sensitive = true
}
variable "DEFAULT_ENCRYPTION_IV" {
  type = string
  sensitive = true
}

variable "DATA_MIGRATION_PASSWORD" {
  type = string
  sensitive = true
}

variable "COMPANIES_HOUSE_API_KEY" {
  type = string
  sensitive = true
}

variable "GOV_UK_NOTIFY_API_KEY" {
  type = string
  sensitive = true
}

variable "BASIC_AUTH_USERNAME" {
  type = string
  default = ""
  sensitive = true
}
variable "BASIC_AUTH_PASSWORD" {
  type = string
  default = ""
  sensitive = true
}

variable "EHRC_API_TOKEN" {
  type = string
  sensitive = true
}

variable "OFFSET_CURRENT_DATE_TIME_FOR_SITE" {
  type = string
  default = ""
}

variable "MAINTENANCE_MODE" {
  type = string
  default = ""
}
variable "MAINTENANCE_MODE_UP_AGAIN_TIME" {
  type = string
  default = ""
}
