
locals {
  postgres_db_name = "gpg"
  postgres_username = "gpg_user"
}

resource "aws_db_subnet_group" "db_subnet_group_for_postgres_db" {
  name = lower("${var.service_name}__${var.environment}__DB_Subnet_Group")
  subnet_ids = [aws_subnet.vpc_main__public_subnet_az1.id, aws_subnet.vpc_main__public_subnet_az2.id]
}

resource "aws_rds_cluster" "aurora_cluster" {
  cluster_identifier = lower("${var.service_name_hyphens}-${var.environment_hyphens}-Aurora-Cluster")

  // Engine
  engine = "aurora-postgresql"
  engine_mode = "provisioned"  // "Serverless v2 uses the provisioned engine_mode"  https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/rds_cluster#rds-serverless-v2-cluster
  engine_version = "16.3"

  // Sizing
  serverlessv2_scaling_configuration {
    min_capacity = 0.5
    max_capacity = 8
  }
  
  // Connection & security details
  database_name = local.postgres_db_name
  port = 5432
  master_username = local.postgres_username
  master_password = var.POSTGRES_PASSWORD
  storage_encrypted = true

  
  // Networking
  db_subnet_group_name = aws_db_subnet_group.db_subnet_group_for_postgres_db.name
  vpc_security_group_ids = [aws_security_group.security_group_database.id]

  // Upgrades
  allow_major_version_upgrade = false
  preferred_maintenance_window = "Mon:04:00-Mon:05:00"

  // Backups and deletion
  deletion_protection = true
  backup_retention_period = 35
  preferred_backup_window = "02:00-03:00"
  copy_tags_to_snapshot = true
  delete_automated_backups = false
  skip_final_snapshot = false
  final_snapshot_identifier = "${var.service_name_hyphens}-${var.environment_hyphens}-Postgres-Database-Final-Snapshot"

  // Logging & monitoring
  enabled_cloudwatch_logs_exports = ["postgresql"]
}

resource "aws_rds_cluster_instance" "aurora_cluster_instance" {
  cluster_identifier = aws_rds_cluster.aurora_cluster.id

  // Engine
  engine = aws_rds_cluster.aurora_cluster.engine
  engine_version = aws_rds_cluster.aurora_cluster.engine_version

  // Sizing
  instance_class = "db.serverless"
  
  // Networking
  db_subnet_group_name = aws_rds_cluster.aurora_cluster.db_subnet_group_name
  publicly_accessible = true
  
  // Upgrades
  auto_minor_version_upgrade = true
  preferred_maintenance_window = "Mon:04:00-Mon:05:00"

  // Backups and deletion
  // set on the cluster

  // Logging & monitoring
  monitoring_role_arn = aws_iam_role.rds_enhanced_monitoring.arn
  monitoring_interval = 60
}

resource "aws_iam_role" "rds_enhanced_monitoring" {
  name_prefix = "rds-enhanced-monitoring-"
  assume_role_policy = data.aws_iam_policy_document.rds_enhanced_monitoring.json
}

resource "aws_iam_role_policy_attachment" "rds_enhanced_monitoring" {
  role = aws_iam_role.rds_enhanced_monitoring.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
}

data "aws_iam_policy_document" "rds_enhanced_monitoring" {
  statement {
    actions = [
      "sts:AssumeRole",
    ]

    effect = "Allow"

    principals {
      type = "Service"
      identifiers = ["monitoring.rds.amazonaws.com"]
    }
  }
}