// no failover, single node, daily backups, 1.34gb (step down from 1.5gb)

resource "aws_elasticache_cluster" "redis-cluster" {
  cluster_id           = "gpg-redis-cluster-${var.env}"
  engine               = "redis"
  node_type            = "cache.t4g.small"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis6.x"
  port                 = var.cache_port
  security_group_ids   = [module.vpc.default_security_group_id]
  subnet_group_name    = module.vpc.elasticache_subnet_group_name
}