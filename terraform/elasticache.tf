// no failover, single node, daily backups, 1.34gb (step down from 1.5gb)

resource "aws_elasticache_cluster" "redis-cluster" {
  cluster_id           = "gpg-redis-cluster-${var.env}"
  engine               = "redis"
  node_type            = "cache.t4g.small"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis6.x"
  port                 = var.cache_port
  security_group_ids   = [aws_security_group.elasticache_security_group.id]
  subnet_group_name    = module.vpc.elasticache_subnet_group_name

  log_delivery_configuration {
    destination      = aws_cloudwatch_log_group.redis.name
    destination_type = "cloudwatch-logs"
    log_format       = "text"
    log_type         = "slow-log"
  }
  log_delivery_configuration {
    destination      = aws_cloudwatch_log_group.redis.name
    destination_type = "cloudwatch-logs"
    log_format       = "text"
    log_type         = "engine-log"
  }
  apply_immediately = true
  // turn this off after testing
}

resource "aws_cloudwatch_log_group" "redis" {
  name = "redis-logs-${var.env}"
}

resource "aws_security_group" "elasticache_security_group" {
  name   = "elasticache-${var.env}"
  vpc_id = module.vpc.vpc_id

  ingress {
    description      = "TLS from VPC"
    from_port        = 6379
    to_port          = 6379
    protocol         = "tcp"
    cidr_blocks      = [module.vpc.vpc_cidr_block]
    ipv6_cidr_blocks = [module.vpc.vpc_ipv6_cidr_block]
  }

  egress {
    from_port        = 0
    to_port          = 0
    protocol         = "-1"
    cidr_blocks      = ["0.0.0.0/0"]
    ipv6_cidr_blocks = ["::/0"]
  }
}