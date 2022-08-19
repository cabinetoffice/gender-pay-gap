// declarations for the resources to be created 
// this does not include the state S3 bucket as this must be created in order for terraform to init

resource "aws_security_group" "allow_postgres_connection" {
  name        = "allow_postgres_connection"
  description = "Allow Postgres DB inbound traffic"
  vpc_id      = "vpc-0e9491851e4208470"

  tags = {
    Name = "allow_postgres_connection"
  }
}

resource "aws_security_group_rule" "postgres_in" {
  security_group_id = aws_security_group.allow_postgres_connection.id
  type              = "ingress"
  from_port         = 5432
  to_port           = 5432
  protocol          = "tcp"
  cidr_blocks = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "postgres_out" {
  security_group_id = aws_security_group.allow_postgres_connection.id
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks = ["0.0.0.0/0"]
}

resource "aws_db_instance" "gpg-dev-db" {
  allocated_storage           = var.postgres_config.allocated_storage
  engine                      = var.postgres_config.engine
  engine_version              = var.postgres_config.engine_version
  instance_class              = var.postgres_config.instance_class
  identifier                  = var.postgres_config.identifier
  db_name                     = var.postgres_config.db_name
  username                    = var.postgres_config.username
  password                    = var.postgres_config.password
  port                        = var.postgres_config.port
  backup_retention_period     = var.postgres_config.backup_retention_period
  backup_window               = var.postgres_config.backup_window
  vpc_security_group_ids      = [aws_security_group.allow_postgres_connection.id]
  storage_encrypted           = var.postgres_config.storage_encrypted
  publicly_accessible         = var.postgres_config.publicly_accessible
  allow_major_version_upgrade = var.postgres_config.allow_major_version_upgrade
}