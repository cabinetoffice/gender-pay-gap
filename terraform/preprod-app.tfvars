postgres_config = {
  instance_class = "db.t3.small"
  identifier     = "gpg-preprod-db"
  db_name        = "gpgPreprodDb"
  username       = "postgres"
  password       = "postgres"
}

env = "preprod"