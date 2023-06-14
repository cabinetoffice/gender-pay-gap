locals {
  env_prefix     = "gpg-${var.env}"     // prefix env specific resources
  account_prefix = "gpg-${var.account}" // prefix account specific resources

  elb_environment_tier         = "WebServer"
  elb_lb_scheme                = "public"
  elb_load_balancer_ssl_policy = "ELBSecurityPolicy-2016-08"
  elb_load_balancer_type       = "application"
  elb_solution_stack_name      = "64bit Amazon Linux 2 v2.4.0 running .NET Core"
  elb_health_check_path        = "/health-check"
  elb_matcher_http_code        = 200

  managed_policy_arns = ["arn:aws:iam::aws:policy/AWSElasticBeanstalkMulticontainerDocker", "arn:aws:iam::aws:policy/AWSElasticBeanstalkWebTier", "arn:aws:iam::aws:policy/AmazonRDSFullAccess", "arn:aws:iam::aws:policy/AmazonSSMFullAccess", "arn:aws:iam::aws:policy/AWSElasticBeanstalkWorkerTier","arn:aws:iam::aws:policy/AmazonSSMManagedEC2InstanceDefaultPolicy"]
}

data "aws_iam_instance_profile" "elastic_beanstalk" {
  name = "aws-elasticbeanstalk-ec2-role"
}

// Load balancer id
data "aws_instance" "elb_primary_instance" {
  instance_id = aws_elastic_beanstalk_environment.gpg_elastic_beanstalk_environment.instances[0]
}

//S3 bucket containing application versions for all env in account
data "aws_s3_bucket" "gpg_application_version_storage" {
  bucket = "${local.account_prefix}-application-version-storage"
}

// File storage bucket for each env
resource "aws_s3_bucket" "gpg_filestorage" {
  bucket = "${local.env_prefix}-filestorage"
  
  lifecycle {
    prevent_destroy = false // QQ turn to true when live
  }
}

resource "aws_s3_bucket_versioning" "gpg-filestorage" {
  bucket = aws_s3_bucket.gpg_filestorage.id
  versioning_configuration {
    status = "Enabled"
  }
}

// Archive file 
data "aws_s3_object" "gpg_archive_zip" {
  bucket = data.aws_s3_bucket.gpg_application_version_storage.id
  key    = "publish-${var.env}.zip"
}

// Application
resource "aws_elastic_beanstalk_application" "gpg_application" {
  name        = "${local.env_prefix}-application"
  description = "The GPG application in ${var.env}."
}

// Application version
resource "aws_elastic_beanstalk_application_version" "gpg_application_version" {
  name        = "${local.env_prefix}-version"
  application = aws_elastic_beanstalk_application.gpg_application.name
  description = "The application version used to create the elastic beanstalk resource."
  bucket      = data.aws_s3_bucket.gpg_application_version_storage.bucket
  key         = data.aws_s3_object.gpg_archive_zip.key
}

// Elastic beanstalk environment
resource "aws_elastic_beanstalk_environment" "gpg_elastic_beanstalk_environment" {
  name                = "${local.env_prefix}-elb-environment"
  application         = aws_elastic_beanstalk_application.gpg_application.name
  solution_stack_name = local.elb_solution_stack_name
  version_label       = aws_elastic_beanstalk_application_version.gpg_application_version.name
  cname_prefix        = local.env_prefix // must check availability in console before changing
  
  // Life cycle methods
  lifecycle {
    prevent_destroy = false // QQ turn to true when live
  }

  // See this documentation for all the available settings
  // https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/command-options-general.html

  // Deployment strategy
  setting {
    namespace = "aws:elasticbeanstalk:command"
    name      = "DeploymentPolicy"
    value     = var.elb_deployment_policy
  }

  // Elastic beanstalk VPC config
  setting {
    namespace = "aws:ec2:vpc"
    name      = "DBSubnets"
    value     = join(",", module.vpc.database_subnets)
  }

  setting {
    namespace = "aws:ec2:vpc"
    name      = "Subnets"
    value     = join(",", module.vpc.public_subnets)
  }

  setting {
    namespace = "aws:ec2:vpc"
    name      = "VPCId"
    value     = module.vpc.vpc_id
  }

  // Elastic beanstalk load balancer config
  setting {
    namespace = "aws:ec2:vpc"
    name      = "ELBScheme"
    value     = local.elb_lb_scheme
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "LoadBalancerType"
    value     = local.elb_load_balancer_type
  }

  // HTTP listener config

  setting {
    namespace = "aws:elbv2:listener:default"
    name      = "ListenerEnabled"
    value     = "false"  // disabled. we create our own port 80 listener which redirects to https
  }

  // HTTPS secure listener config
  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "ListenerEnabled"
    value     = "true"
  }

  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "Protocol"
    value     = "HTTPS"
  }

  setting {
    namespace = "aws:elbv2:listener:443"
    name      = "SSLCertificateArns"
    value     = var.ELB_LOAD_BALANCER_SSL_CERTIFICATE_ARN
  }
  
  setting {
    namespace = "aws:elbv2:listener:default"
    name = "SSLPolicy"
    value = "ELBSecurityPolicy-2016-08"
  }

  // HTTPS secure listener rules
  setting {
    namespace = "aws:elasticbeanstalk:environment:process:https"
    name      = "HealthCheckPath"
    value     = local.elb_health_check_path
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:https"
    name      = "MatcherHTTPCode"
    value     = local.elb_matcher_http_code
  }

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
    value     = data.aws_iam_instance_profile.elastic_beanstalk.name
  }

  setting {
    namespace = "aws:ec2:instances"
    name      = "InstanceTypes"
    value     = var.elb_instance_type
  }

  setting {
    namespace = "aws:autoscaling:asg"
    name      = "MaxSize"
    value     = var.elb_instance_max_size
  }

  setting {
    namespace = "aws:autoscaling:asg"
    name      = "MinSize"
    value     = var.elb_instance_min_size
  }

  // Auto-scaling Triggers
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "MeasureName"
    value     = "CPUUtilization"
  }
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "Statistic"
    value     = "Average"
  }
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "Unit"
    value     = "Percent"
  }
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "Period"
    value     = 1  // Time (in minutes) between checks
                   // Note: remember to update the other settings
  }                // BreachDuration = Period * EvaluationPeriods
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "EvaluationPeriods"
    value     = 3  // Number of consecutive checks that must be too high/low to trigger a scaling action
                   // Note: remember to update the other settings
  }                // BreachDuration = Period * EvaluationPeriods
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "BreachDuration"
    value     = 3  // How long (in minutes) must the checks be toon high/low before scaling up/down
                   // Note: remember to update the other settings
  }                // BreachDuration = Period * EvaluationPeriods
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "UpperThreshold"
    value     = 80  // If the CPU % stays above this level, we scale up
  }
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "UpperBreachScaleIncrement"
    value     = 1  // How many instances to add when we scale up
  }
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "LowerThreshold"
    value     = 50  // If the CPU % stays below this level, we scale down
  }
  setting {
    namespace = "aws:autoscaling:trigger"
    name      = "LowerBreachScaleIncrement"
    value     = -1  // How many instances to ADD when we scale down
  }                 // (this needs to be a negative number so we scale down!)

  // Elastic beanstalk static assets config
  setting {
    namespace = "aws:elasticbeanstalk:environment:proxy:staticfiles"
    name      = "/images"
    value     = "wwwroot/assets/images"
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:proxy:staticfiles"
    name      = "/public"
    value     = "wwwroot/public"
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:proxy:staticfiles"
    name      = "/compiled"
    value     = "wwwroot/compiled"
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:proxy:staticfiles"
    name      = "/assets"
    value     = "wwwroot/assets"
  }

  // Elastic beanstalk log config
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

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs"
    name      = "StreamLogs"
    value     = true
  }

  // Elastic beanstalk health check config
  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs:health"
    name      = "DeleteOnTerminate"
    value     = false
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
    name      = "HealthCheckPath"
    value     = local.elb_health_check_path
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "HealthCheckTimeout"
    value     = 5
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs:health"
    name      = "HealthStreamingEnabled"
    value     = true
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "HealthyThresholdCount"
    value     = 3
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "MatcherHTTPCode"
    value     = local.elb_matcher_http_code
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs:health"
    name      = "RetentionInDays"
    value     = 7
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "StickinessEnabled"
    value     = true
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment:process:default"
    name      = "UnhealthyThresholdCount"
    value     = 5
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
      aws-s3-bucket = [{
        name = aws_s3_bucket.gpg_filestorage.bucket,
        credentials = {
          aws_region            = var.aws_region,
          bucket_name           = aws_s3_bucket.gpg_filestorage.bucket,
          aws_access_key_id     = var.AWS_ACCESS_KEY_ID,
          aws_secret_access_key = var.AWS_SECRET_ACCESS_KEY
        }
      }]
    })
  }

  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DATABASE_HOST"
    value     = aws_db_instance.gpg-dev-db.address
  }
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DATABASE_PORT"
    value     = aws_db_instance.gpg-dev-db.port
  }
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DATABASE_USERNAME"
    value     = aws_db_instance.gpg-dev-db.username
  }
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DATABASE_PASSWORD"
    value     = aws_db_instance.gpg-dev-db.password
  }
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "DATABASE_DB_NAME"
    value     = aws_db_instance.gpg-dev-db.name
  }
  
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "S3_BUCKET_NAME"
    value     = aws_s3_bucket.gpg_filestorage.bucket
  }
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "S3_BUCKET_AWS_ACCESS_KEY_ID"
    value     = var.AWS_ACCESS_KEY_ID
  }
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "S3_BUCKET_AWS_SECRET_ACCESS_KEY"
    value     = var.AWS_SECRET_ACCESS_KEY
  }
  setting {
    namespace = "aws:elasticbeanstalk:application:environment"
    name      = "S3_BUCKET_AWS_REGION"
    value     = var.aws_region
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

