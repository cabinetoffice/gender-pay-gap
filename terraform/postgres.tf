locals {
  rds_config_port = 5432
}

// The security group the database will belong to
resource "aws_security_group" "allow_postgres_connection" {
  name        = "${local.env_prefix}-allow_postgres_connection"
  description = "Allow Postgres DB traffic"
  vpc_id      = module.vpc.vpc_id
}

// Incoming rules
resource "aws_security_group_rule" "postgres_in" {
  security_group_id = aws_security_group.allow_postgres_connection.id
  type              = "ingress"
  from_port         = local.rds_config_port
  to_port           = local.rds_config_port
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
  allocated_storage           = 100
  engine                      = "postgres"
  engine_version              = 14
  instance_class              = var.rds_config_instance_class
  identifier                  = var.rds_config_identifier
  db_name                     = var.rds_config_db_name
  port                        = local.rds_config_port
  username                    = var.POSTGRES_CONFIG_USERNAME
  password                    = var.POSTGRES_CONFIG_PASSWORD
  backup_retention_period     = 30
  backup_window               = "04:00-05:00"
  vpc_security_group_ids      = [aws_security_group.allow_postgres_connection.id]
  db_subnet_group_name        = module.vpc.database_subnet_group_name
  storage_encrypted           = true
  publicly_accessible         = false
  allow_major_version_upgrade = false
  multi_az                    = var.rds_config_multi_az
  skip_final_snapshot         = false
  final_snapshot_identifier   = join("-", [var.rds_config_identifier, "final-snapshot", replace(timestamp(), ":", "-")])

  // Backups and deletion 
  deletion_protection      = false // QQ should be true when application goes live
  delete_automated_backups = true  //  QQ should be false when in production

  lifecycle {
    ignore_changes = [
      // This will always be different because of the timestamp function, so we ignore it
      final_snapshot_identifier
    ]
  }

  // Logging and monitoring
  enabled_cloudwatch_logs_exports = ["postgresql"]
  monitoring_interval             = 60
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
