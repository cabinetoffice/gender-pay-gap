// declarations for the resources to be created 
// this does not include the state S3 bucket as this must be created in order for terraform to init

resource "aws_db_instance" "gpg-dev-db" {
  allocated_storage       = 100
  engine                  = "postgres"
  engine_version          = "11"
  instance_class          = "db.t3.small"
  identifier              = "gpg-dev-db"
  db_name                 = "gpg-dev-db"
  username                = "postgres"
  password                = "postgres"
  port                    = 5432
  backup_retention_period = 5
  backup_window           = "04:00-05:00"
  storage_encrypted       = true
}