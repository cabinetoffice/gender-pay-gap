// declarations for the resources to be created 
// this does not include the state S3 bucket as this must be created in order for terraform to init

data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_lb" "load-balancer" {
  arn = aws_elastic_beanstalk_environment.gpg-elb-environment.load_balancers[0]
}