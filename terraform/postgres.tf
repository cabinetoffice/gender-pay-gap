resource "aws_security_group" "allow_postgres_connection" {
  name        = join("_", ["allow_postgres_connection", var.env])
  description = "Allow Postgres DB traffic"
  vpc_id      = module.vpc.vpc_id
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
  db_subnet_group_name        = module.vpc.database_subnet_group_name
  storage_encrypted           = local.postgres_config.storage_encrypted
  publicly_accessible         = local.postgres_config.publicly_accessible
  allow_major_version_upgrade = local.postgres_config.allow_major_version_upgrade
  multi_az                    = local.postgres_config.multi_az
  skip_final_snapshot         = local.postgres_config.skip_final_snapshot
  final_snapshot_identifier   = local.postgres_config.final_snapshot_identifier
  lifecycle {
    ignore_changes = [
      final_snapshot_identifier
    ]
  }
}