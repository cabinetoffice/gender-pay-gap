env = "dev"
application_version_s3_bucket = "gpg-preproduction-application-version-storage"

#region Relational database configuration 

rds_config_db_name        = "gpgDevDb"
rds_config_identifier     = "gpg-dev-db"
rds_config_instance_class = "db.t3.small"
rds_config_multi_az       = false

#endregion

#region Elastic Beanstalk configuration

elb_cname_prefix        = "dev-gpg"
elb_deployment_policy   = "Rolling"
elb_instance_max_size   = 2
elb_instance_min_size   = 1
elb_instance_profile    = "aws-elasticbeanstalk-ec2-role-dev"
elb_instance_type       = "t2.small"
elb_lb_scheme           = "public"
elb_load_balancer_type  = "application"
elb_solution_stack_name = "64bit Amazon Linux 2 v2.4.0 running .NET Core"
elb_ssl_policy          = "ELBSecurityPolicy-2016-08"
elb_tier                = "WebServer"

#endregion

#region Elasticache configuration

elasticache_cache_port = 6379

#endregion

#region Cloudfront configuration

cloudfront_alternate_domain_name = "gender-pay-gap-test.codatt.net"
cloudfront_logging_prefix        = "cloudfront-dev"
cloudfront_origin_id             = "gpg-load-balancer-dev"

#end region