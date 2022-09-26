#region Relational database configuration 

rds_config_instance_class = "db.t3.small"
rds_config_identifier     = "gpg-prod-db"
rds_config_db_name        = "gpgProdDb"
rds_config_multi_az       = true

#endregion

env = "prod"

#region ElasticBeanstalk configuration

elb_instance_type          = "t2.medium"
elb_instance_profile   = "aws-elasticbeanstalk-ec2-role"
elb_instance_min_size  = 1
elb_instance_max_size  = 4
tier                   = "WebServer"
elb_scheme             = "internet facing"
elb_load_balancer_type = "application"
solution_stack_name    = "64bit Amazon Linux 2 v2.3.4 running .NET Core"
elb_matcher_http_code  = "200"

#endregion
