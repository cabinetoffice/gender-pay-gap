// all declared input variables

variable "bucket_prefix" {
  type        = string
  description = "Creates a unique bucket name beginning with the specified prefix. Conflicts with bucket"
  default     = ""
}

variable "acl" {
  type        = string
  description = "(Optional) Defaults to private. Conflicts with grant."
  default     = "private"
}

variable "versioning" {
  type        = bool
  description = "(Optional) A state of versioning."
  default     = true
}

variable "aws_region" {
  description = "The AWS region to use to create resources."
  default     = "eu-west-2"
}