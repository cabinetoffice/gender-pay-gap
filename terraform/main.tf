// declarations for the resources to be created 
// this does not include the state S3 bucket as this must be created in order for terraform to init

locals {
  postgres_config = defaults(var.postgres_config, {
    allocated_storage           = 100
    engine                      = "postgres"
    engine_version              = "14"
    username                    = var.postgres_config_username
    password                    = var.postgres_config_password
    port                        = 5432
    backup_retention_period     = 30
    backup_window               = "04:00-05:00"
    storage_encrypted           = true
    publicly_accessible         = true
    allow_major_version_upgrade = true
    multi_az                    = false
    skip_final_snapshot         = false
    final_snapshot_identifier   = join("-", [var.postgres_config.identifier, "final-snapshot"])
  })
}

resource "aws_security_group" "allow_postgres_connection" {
  name        = join("_", ["allow_postgres_connection", var.env])
  description = "Allow Postgres DB traffic"
  vpc_id      = "vpc-0e9491851e4208470"
}

resource "aws_security_group_rule" "postgres_in" {
  security_group_id = aws_security_group.allow_postgres_connection.id
  type              = "ingress"
  from_port         = local.postgres_config.port
  to_port           = local.postgres_config.port
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "postgres_out" {
  security_group_id = aws_security_group.allow_postgres_connection.id
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_db_instance" "gpg-dev-db" {
  allocated_storage           = local.postgres_config.allocated_storage
  engine                      = local.postgres_config.engine
  engine_version              = local.postgres_config.engine_version
  instance_class              = local.postgres_config.instance_class
  identifier                  = local.postgres_config.identifier
  db_name                     = local.postgres_config.db_name
  username                    = local.postgres_config.username
  password                    = local.postgres_config.password
  port                        = local.postgres_config.port
  backup_retention_period     = local.postgres_config.backup_retention_period
  backup_window               = local.postgres_config.backup_window
  vpc_security_group_ids      = [aws_security_group.allow_postgres_connection.id]
  storage_encrypted           = local.postgres_config.storage_encrypted
  publicly_accessible         = local.postgres_config.publicly_accessible
  allow_major_version_upgrade = local.postgres_config.allow_major_version_upgrade
  multi_az                    = local.postgres_config.multi_az
  skip_final_snapshot         = local.postgres_config.skip_final_snapshot
  final_snapshot_identifier   = local.postgres_config.final_snapshot_identifier
}