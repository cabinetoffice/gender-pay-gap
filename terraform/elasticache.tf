// no failover, single node, daily backups, 1.34gb (step down from 1.5gb)

resource "aws_elasticache_cluster" "redis-cluster" {
  cluster_id           = "gpg-redis-cluster-${var.env}"
  engine               = "redis"
  node_type            = "cache.t4.small"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis3.2"
  engine_version       = "3.2.10"
  port                 = var.cache_port
  security_group_ids   = [module.vpc.default_security_group_id]
}