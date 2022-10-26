// The security group the database will belong to
resource "aws_security_group" "allow_postgres_connection" {
  name        = "allow_postgres_connection-${var.env}"
  description = "Allow Postgres DB traffic"
  vpc_id      = module.vpc.vpc_id
}

// Incoming rules
resource "aws_security_group_rule" "postgres_in" {
  security_group_id = aws_security_group.allow_postgres_connection.id
  type              = "ingress"
  from_port         = var.rds_config_port
  to_port           = var.rds_config_port
  protocol          = "tcp"
  cidr_blocks       = [module.vpc.vpc_cidr_block]
  ipv6_cidr_blocks  = [module.vpc.vpc_ipv6_cidr_block]
}

// Outgoing rules
resource "aws_security_group_rule" "postgres_out" {
  security_group_id = aws_security_group.allow_postgres_connection.id
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = [module.vpc.vpc_cidr_block]
  ipv6_cidr_blocks  = [module.vpc.vpc_ipv6_cidr_block]
}

// The database resource
resource "aws_db_instance" "gpg-dev-db" {
  allocated_storage           = var.rds_config_allocated_storage
  engine                      = var.rds_config_engine
  engine_version              = var.rds_config_engine_version
  instance_class              = var.rds_config_instance_class
  identifier                  = var.rds_config_identifier
  db_name                     = var.rds_config_db_name
  username                    = var.POSTGRES_CONFIG_USERNAME
  password                    = var.POSTGRES_CONFIG_PASSWORD
  port                        = var.rds_config_port
  backup_retention_period     = var.rds_config_backup_retention_period
  backup_window               = var.rds_config_backup_window
  vpc_security_group_ids      = [aws_security_group.allow_postgres_connection.id]
  db_subnet_group_name        = module.vpc.database_subnet_group_name
  storage_encrypted           = var.rds_config_storage_encrypted
  publicly_accessible         = var.rds_config_publicly_accessible
  allow_major_version_upgrade = var.rds_config_allow_major_version_upgrade
  multi_az                    = var.rds_config_multi_az
  skip_final_snapshot         = var.rds_config_skip_final_snapshot
  final_snapshot_identifier   = join("-", [var.rds_config_identifier, "final-snapshot", replace(timestamp(), ":", "-")])

  // Backups and deletion 
  deletion_protection      = false // should be true when application goes live
  delete_automated_backups = true  // should be false when in production

  lifecycle {
    ignore_changes = [
      // This will always be different because of the timestamp function, so we ignore it
      final_snapshot_identifier
    ]
  }

  // Logging and monitoring
  enabled_cloudwatch_logs_exports = ["postgresql"]
  monitoring_interval             = var.rds_config_monitoring_interval
  monitoring_role_arn             = aws_iam_role.rds_enhanced_monitoring.arn
}

resource "aws_iam_role" "rds_enhanced_monitoring" {
  name_prefix        = "rds-enhanced-monitoring-"
  assume_role_policy = data.aws_iam_policy_document.rds_enhanced_monitoring.json
}

resource "aws_iam_role_policy_attachment" "rds_enhanced_monitoring" {
  role       = aws_iam_role.rds_enhanced_monitoring.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
}

data "aws_iam_policy_document" "rds_enhanced_monitoring" {
  statement {
    actions = [
      "sts:AssumeRole",
    ]

    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["monitoring.rds.amazonaws.com"]
    }
  }
}
