postgres_config = {
  instance_class = "db.t3.small"
  identifier     = "gpg-prod-db"
  db_name        = "gpgProdDb"
  multi_az       = true
}

env = "prod"