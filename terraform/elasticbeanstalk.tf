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


  setting {
    namespace = "aws:ec2:vpc"
    name      = "VPCId"
    value     = module.vpc.vpc_id
  }

  setting {
    namespace = "aws:ec2:vpc"
    name      = "Subnets"
    value     = join(",", module.vpc.public_subnets)
  }

  setting {
    namespace = "aws:ec2:vpc"
    name      = "DBSubnets"
    value     = join(",", module.vpc.database_subnets)
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "VCAP_SERVICES"
    value     = var.ELB_VCAP_SERVICES
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "AdminEmails"
    value     = var.ELB_ADMIN_EMAILS
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "ASPNETCORE_ENVIRONMENT"
    value     = var.ELB_ASPNETCORE_ENVIRONMENT
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "CompaniesHouseApiKey"
    value     = var.ELB_COMPANIES_HOUSE_API_KEY
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DefaultEncryptionKey"
    value     = var.ELB_DEFAULT_ENCRYPTION_KEY
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "EhrcIPRange"
    value     = var.ELB_EHRC_IP_RANGE
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "GEODistributionList"
    value     = var.ELB_GEO_DISTRIBUTION_LIST
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "GovUkNotifyApiKey"
    value     = var.ELB_GOVUK_NOTIFY_API_KEY
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "GpgAnalysisAppApiPassword"
    value     = var.ELB_GPG_ANALYSIS_APP_API_PASSWORD
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "ObfuscationSeed"
    value     = var.ELB_OBFUSCATION_SEED
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "WEBJOBS_STOPPED"
    value     = var.ELB_WEBJOBS_STOPPED
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DataMigrationPassword"
    value     = var.ELB_DATA_MIGRATION_PASSWORD
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DisableSearchCache"
    value     = var.ELB_DISABLE_SEARCH_CACHE
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "BasicAuthUsername"
    value     = var.ELB_BASIC_AUTH_USERNAME
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "BasicAuthPassword"
    value     = var.ELB_BASIC_AUTH_PASSWORD
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "FeatureFlagNewReportingJourney"
    value     = var.ELB_FEATURE_FLAG_NEW_REPORTING_JOURNEY
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "ReportingStartYearsToExcludeFromLateFlagEnforcement"
    value     = var.ELB_REPORTING_START_YEARS_TO_EXCLUDE_FROM_LATE_FLAG_ENFORCEMENT
  }

}
