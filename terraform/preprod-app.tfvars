#region Relational database configuration 

rds_config_instance_class = "db.t3.small"
rds_config_identifier     = "gpg-preprod-db"
rds_config_db_name        = "gpgPreprodDb"

#endregion

env = "preprod"

#region ElasticBeanstalk configuration

instance_type             = "t2.small"
elb_instance_profile      = "aws-elasticbeanstalk-ec2-role"
elb_instance_min_size     = 1
elb_instance_max_size     = 2
tier                      = "WebServer"
elb_scheme                = "internet facing"
elb_load_balancer_type    = "application"
solution_stack_name       = "64bit Amazon Linux 2 v2.4.0 running .NET Core"
cache_port                = 6379
cname_prefix              = "gpg-preprod"
elb_ssl_policy            = "ELBSecurityPolicy-TLS-1-2-Ext-2018-06"
cloudfront_origin_id      = "gpg-load-balancer-preprod"
route_53_domain           = "test.gender-pay-gap.service.gov.uk"
cloudfront_logging_prefix = "cloudfront-preprod"

#endregion
