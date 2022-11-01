env     = "prod"
account = "production"

#region Relational database configuration 

rds_config_instance_class = "db.t3.small"
rds_config_identifier     = "gpg-prod-db"
rds_config_db_name        = "gpgProdDb"
rds_config_multi_az       = true

#endregion

#region Elastic Beanstalk configuration

elb_deployment_policy = "Rolling"
elb_instance_max_size = 4
elb_instance_min_size = 1
elb_instance_type     = "t2.medium"

#endregion

#region Cloudfront configuration

cloudfront_alternate_domain_name = "gender-pay-gap.service.gov.uk"

#endregion
