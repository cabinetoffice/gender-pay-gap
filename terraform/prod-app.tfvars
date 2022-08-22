postgres_config = {
  instance_class = "db.t3.small"
  identifier     = "gpg-prod-db"
  db_name        = "gpgProdDb"
  username       = "postgres"
  password       = "postgres"
  multi_az       = true
}

env = "prod"