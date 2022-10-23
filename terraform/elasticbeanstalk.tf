//S3 bucket containing application version
data "aws_s3_bucket" "gpg-application-version-storage" {
  bucket = "gpg-application-version-storage"
}

resource "aws_s3_bucket" "gpg-filestorage" {
  bucket = "gpg-${var.env}-filestorage"
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

// Elastic beanstalk environment
resource "aws_elastic_beanstalk_environment" "gpg-elb-environment" {
  name                = "gpg-elb-environment-${var.env}"
  application         = aws_elastic_beanstalk_application.gpg-application.name
  solution_stack_name = var.solution_stack_name
  version_label       = aws_elastic_beanstalk_application_version.gpg-application-version.name
  cname_prefix        = var.cname_prefix

  // Elastic beanstalk VPC config
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

  // Elastic beanstalk load balancer config
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

  // HTTPS secure listener
  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "Protocol"
    value     = "HTTPS"
  }

  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "ListenerEnabled"
    value     = "true"
  }

  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "DefaultProcess"
    value     = "default"
  }

  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "SSLCertificateArns"
    value     = var.ELB_LOAD_BALANCER_SSL_CERTIFICATE_ARNS
  }

  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "SSLPolicy"
    value     = var.elb_ssl_policy
  }

  // HTTPS secure listener matching process
  setting {
    namespace = "aws:elasticbeanstalk:environment:process:https"
    name      = "Port"
    value     = "443"
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:https"
    name      = "Protocol"
    value     = "HTTPS"
  }

  // Elastic beanstalk autoscaling config
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

  // Elastic beanstalk static assets config
  setting {
    namespace = "aws:elasticbeanstalk:environment:proxy:staticfiles"
    name      = "/images"
    value     = "wwwroot/assets/images"
  }

  // Elastic beanstalk log config
  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs"
    name      = "StreamLogs"
    value     = true
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs"
    name      = "DeleteOnTerminate"
    value     = false
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs"
    name      = "RetentionInDays"
    value     = 7
  }

  // Elastic beanstalk health check config
  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "MatcherHTTPCode"
    value     = var.elb_matcher_http_code
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:https"
    name      = "MatcherHTTPCode"
    value     = var.elb_matcher_http_code
  }
  
  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "HealthCheckPath"
    value     = "/"
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "DeregistrationDelay"
    value     = 20
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "HealthCheckInterval"
    value     = 15
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "HealthCheckTimeout"
    value     = 5
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "HealthyThresholdCount"
    value     = 3
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "UnhealthyThresholdCount"
    value     = 5
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "Port"
    value     = 80
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "Protocol"
    value     = "HTTP"
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "StickinessEnabled"
    value     = false
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs:health"
    name      = "HealthStreamingEnabled"
    value     = true
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs:health"
    name      = "RetentionInDays"
    value     = 7
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs:health"
    name      = "DeleteOnTerminate"
    value     = false
  }

  // Elastic beanstalk environment variables
  // VCAP services is a legacy object from PaaS
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "VCAP_SERVICES"
    value = jsonencode({
      postgres = [{
        credentials = {
          password = aws_db_instance.gpg-dev-db.password,
          port     = aws_db_instance.gpg-dev-db.port,
          name     = aws_db_instance.gpg-dev-db.name,
          host     = aws_db_instance.gpg-dev-db.address,
          username = aws_db_instance.gpg-dev-db.username
        },
        // Identifier ends in -db, which is used by Global.cs to fetch the configuration
        name = aws_db_instance.gpg-dev-db.identifier
      }],
      redis = [{
        credentials = {
          port = aws_elasticache_cluster.redis-cluster.port,
          host = aws_elasticache_cluster.redis-cluster.cache_nodes[0].address
        }
        name = "gpg-${var.env}-cache"
      }],
      aws-s3-bucket = [{
        name = aws_s3_bucket.gpg-filestorage.bucket,
        credentials = {
          aws_region            = var.aws_region,
          bucket_name           = aws_s3_bucket.gpg-filestorage.bucket,
          aws_access_key_id     = var.AWS_ACCESS_KEY_ID,
          aws_secret_access_key = var.AWS_SECRET_ACCESS_KEY
        }
      }]
    })
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
    name      = "BasicAuthPassword"
    value     = var.ELB_BASIC_AUTH_PASSWORD
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "BasicAuthUsername"
    value     = var.ELB_BASIC_AUTH_USERNAME
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "CompaniesHouseApiKey"
    value     = var.ELB_COMPANIES_HOUSE_API_KEY
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DataMigrationPassword"
    value     = var.ELB_DATA_MIGRATION_PASSWORD
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DefaultEncryptionKey"
    value     = var.ELB_DEFAULT_ENCRYPTION_KEY
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DisableSearchCache"
    value     = var.ELB_DISABLE_SEARCH_CACHE
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "EhrcIPRange"
    value     = var.ELB_EHRC_IP_RANGE
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "EnableConsoleLogging"
    value     = var.ELB_ENABLE_CONSOLE_LOGGING
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "FeatureFlagNewReportingJourney"
    value     = var.ELB_FEATURE_FLAG_NEW_REPORTING_JOURNEY
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "FeatureFlagPrivateManualRegistration"
    value     = var.ELB_FEATURE_FLAG_PRIVATE_MANUAL_REGISTRATION
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "FeatureFlagSendRegistrationReviewEmails"
    value     = var.ELB_FEATURE_FLAG_SEND_REGISTRATION_REVIEW_EMAILS
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
    name      = "OffsetCurrentDateTimeForSite"
    value     = var.ELB_OFFSET_CURRENT_DATE_TIME_FOR_SITE
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "ReminderEmailDays"
    value     = var.ELB_REMINDER_EMAIL_DAYS
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "ReportingStartYearsToExcludeFromLateFlagEnforcement"
    value     = var.ELB_REPORTING_START_YEARS_TO_EXCLUDE_FROM_LATE_FLAG_ENFORCEMENT
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "ReportingStartYearsWithFurloughScheme"
    value     = var.ELB_REPORTING_START_YEARS_WITH_FURLOUGH_SCHEME
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "WEBJOBS_STOPPED"
    value     = var.ELB_WEBJOBS_STOPPED
  }

}

