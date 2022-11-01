env     = "preprod"
account = "preproduction"

#region Relational database configuration 

rds_config_db_name        = "gpgPreprodDb"
rds_config_identifier     = "gpg-preprod-db"
rds_config_instance_class = "db.t3.small"
rds_config_multi_az       = false

#endregion

#region Elastic Beanstalk configuration

elb_deployment_policy = "Rolling"
elb_instance_max_size = 2
elb_instance_min_size = 1
elb_instance_type     = "t2.small"

#endregion

#region Cloudfront configuration

cloudfront_alternate_domain_name = "ladun.me"

#endregion
