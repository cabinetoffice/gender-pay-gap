
//an s3 bucket to keep application versions in
data "aws_s3_bucket" "gpg-application-version-storage" {
  bucket = "gpg-application-version-storage"
  //add some dates?
}

//the archive file from the bucket zip
data "aws_s3_object" "gpg-archive-zip" {
  bucket = data.aws_s3_bucket.gpg-application-version-storage.id
  key    = "publish.zip"
}

//an application
resource "aws_elastic_beanstalk_application" "gpg-application" {
  name        = "gpg-application"
  description = "The GPG application"
}

// and the application version
resource "aws_elastic_beanstalk_application_version" "gpg-application-version" {
  name        = "gpg-version-label"
  application = "gpg-application"
  description = "application version created by terraform"
  bucket      = data.aws_s3_bucket.gpg-application-version-storage.bucket
  key         = data.aws_s3_object.gpg-archive-zip.key
}

//and the environment which is being dynamically configured with all the auto scaling etc settings
resource "aws_elastic_beanstalk_environment" "gpg-elb-environment" {
  name                = "gpg-elb-environment"
  application         = aws_elastic_beanstalk_application.gpg-application.name
  solution_stack_name = var.solution_stack_name
  version_label       = aws_elastic_beanstalk_application_version.gpg-application-version.name

  //instance profile for ec2 instance
  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "IamInstanceProfile"
    value     = var.elb_instance_profile
  }
  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "InstanceType"
    value     = var.elb_instance_type
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

