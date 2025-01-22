
// An S3 bucket for file storage
resource "aws_s3_bucket" "s3_bucket__file_storage" {
  bucket_prefix = lower("${var.service_name_hyphens}--${var.environment_hyphens}--file-storage-")
}

resource "aws_s3_bucket_public_access_block" "file_storage_s3_bucket_public_access_block" {
  bucket = aws_s3_bucket.s3_bucket__file_storage.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}
