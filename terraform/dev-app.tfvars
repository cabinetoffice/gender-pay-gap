postgres_config = {
  allocated_storage           = 100
  engine                      = "postgres"
  engine_version              = "14"
  instance_class              = "db.t3.small"
  identifier                  = "gpg-dev-db"
  db_name                     = "gpgDevDb"
  username                    = "postgres"
  password                    = "postgres"
  port                        = 5432
  backup_retention_period     = 5
  backup_window               = "04:00-05:00"
  storage_encrypted           = true
  publicly_accessible         = true
  allow_major_version_upgrade = true
}