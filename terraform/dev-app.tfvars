Instance_type          = "t2.medium"
elb_instance_profile   = "aws-elasticbeanstalk-ec2-role"
elb_instance_min_size  = 1
elb_instance_max_size  = 2
tier                   = "WebServer"
elb_scheme             = "internet facing"
elb_load_balancer_type = "application"
solution_stack_name    = "64bit Amazon Linux 2 v2.3.4 running .NET Core"
elb_matcher_http_code  = "200"

postgres_config = {
  instance_class = "db.t3.small"
  identifier     = "gpg-dev-db"
  db_name        = "gpgDevDb"
}

env = "dev"