//S3 bucket containing application version
data "aws_s3_bucket" "gpg-application-version-storage" {
  bucket = "gpg-application-version-storage"
}

// Archive file 
data "aws_s3_object" "gpg-archive-zip" {
  bucket = data.aws_s3_bucket.gpg-application-version-storage.id
  key    = "publish-${var.env}.zip"
}

// Application
resource "aws_elastic_beanstalk_application" "gpg-application" {
  name        = "gpg-application-${var.env}"
  description = "The GPG application in ${var.env}"
}

// Application version
resource "aws_elastic_beanstalk_application_version" "gpg-application-version" {
  name        = "gpg-version-label-${var.env}"
  application = aws_elastic_beanstalk_application.gpg-application.name
  description = "application version created by terraform"
  bucket      = data.aws_s3_bucket.gpg-application-version-storage.bucket
  key         = data.aws_s3_object.gpg-archive-zip.key
}

// Beanstalk environment
resource "aws_elastic_beanstalk_environment" "gpg-elb-environment" {
  name                = "gpg-elb-environment-${var.env}"
  application         = aws_elastic_beanstalk_application.gpg-application.name
  solution_stack_name = var.solution_stack_name
  version_label       = aws_elastic_beanstalk_application_version.gpg-application-version.name

  //Instance profile for ec2 instance
  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "IamInstanceProfile"
    value     = var.elb_instance_profile
  }
  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "InstanceType"
    value     = var.instance_type
  }
  setting {
    namespace = "aws:autoscaling:asg"
    name      = "MaxSize"
    value     = var.elb_instance_max_size
  }
  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "LoadBalancerType"
    value     = var.elb_load_balancer_type
  }
  setting {
    namespace = "aws:ec2:vpc"
    name      = "ELBScheme"
    value     = var.elb_scheme
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "MatcherHTTPCode"
    value     = var.elb_matcher_http_code
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "HealthCheckPath"
    value     = "/docs"
  }

}
