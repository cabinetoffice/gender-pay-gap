
locals {
  postgres_config = defaults(var.postgres_config, {
    allocated_storage           = 100
    engine                      = "postgres"
    engine_version              = "14"
    username                    = var.POSTGRES_CONFIG_USERNAME
    password                    = var.POSTGRES_CONFIG_USERNAME
    port                        = 5432
    backup_retention_period     = 30
    backup_window               = "04:00-05:00"
    storage_encrypted           = true
    publicly_accessible         = true
    allow_major_version_upgrade = true
    multi_az                    = false
    skip_final_snapshot         = false
    final_snapshot_identifier   = join("-", [var.postgres_config.identifier, "final-snapshot", replace(timestamp(), ":", "-")])
  })
}