postgres_config = {
  instance_class = "db.t3.small"
  identifier     = "gpg-dev-db"
  db_name        = "gpgDevDb"
  username       = "postgres"
  password       = "postgres"
}

env = "dev"