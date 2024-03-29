// Alarms
resource "aws_cloudwatch_metric_alarm" "no_healthy_hosts" {
  alarm_name          = "${local.env_prefix}-no-healthy-hosts"
  metric_name         = "HealthyHostCount"
  namespace           = "AWS/ApplicationELB"
  comparison_operator = "LessThanOrEqualToThreshold"
  statistic           = "Minimum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  alarm_description   = "${local.env_prefix} has no healthy hosts."
  alarm_actions       = [aws_sns_topic.EC2_topic.arn]
  ok_actions          = [aws_sns_topic.EC2_topic.arn]
  dimensions = {
    LoadBalancer = data.aws_lb.load_balancer.arn
  }
}

resource "aws_cloudwatch_metric_alarm" "http_errors" {
  alarm_name          = "${local.env_prefix}-http-errors"
  metric_name         = "HTTPCode_Target_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  comparison_operator = "GreaterThanThreshold"
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  alarm_description   = "${local.env_prefix} has HTTP 5xx errors."
  alarm_actions       = [aws_sns_topic.EC2_topic.arn]
  ok_actions          = [aws_sns_topic.EC2_topic.arn]
  treat_missing_data  = "notBreaching"
  dimensions = {
    LoadBalancer = data.aws_lb.load_balancer.arn
  }
}

# No instances should fail the health checks unless they failed to boot.
# This usually means a release failed and will need manual intervention.
resource "aws_cloudwatch_metric_alarm" "unhealthy_hosts" {
  alarm_name          = "${local.env_prefix}-unhealthy-hosts"
  metric_name         = "UnHealthyHostCount"
  namespace           = "AWS/ApplicationELB"
  comparison_operator = "GreaterThanThreshold"
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  alarm_description   = "${local.env_prefix} has unhealthy hosts. A release likely failed and will need manual intervention."
  alarm_actions       = [aws_sns_topic.EC2_topic.arn]
  ok_actions          = [aws_sns_topic.EC2_topic.arn]
  treat_missing_data  = "notBreaching"
  dimensions = {
    LoadBalancer = data.aws_lb.load_balancer.arn
  }
}

resource "aws_cloudwatch_metric_alarm" "cpu_utilisation" {
  alarm_name          = "${local.env_prefix}-cpu-too-high"
  metric_name         = "CPUUtilization"
  namespace           = "AWS/EC2"
  comparison_operator = "GreaterThanOrEqualToThreshold"
  statistic           = "Average"
  period              = "60"
  evaluation_periods  = "2"
  threshold           = "70"
  alarm_actions       = [aws_sns_topic.EC2_topic.arn]
  ok_actions          = [aws_sns_topic.EC2_topic.arn]
  treat_missing_data  = "notBreaching"
  alarm_description   = "${local.env_prefix}'s EC2 CPU utilisation is exceeding 70%"
  dimensions = {
    InstanceId = data.aws_instance.elb_primary_instance.id
  }
}

resource "aws_sns_topic" "EC2_topic" {
  name = "${local.env_prefix}-ec2-cloudwatch-alarms"
}

resource "aws_sns_topic_subscription" "EC2_Subscription" {
  topic_arn = aws_sns_topic.EC2_topic.arn
  protocol  = "email"
  endpoint  = var.CLOUDWATCH_NOTIFICATION_EMAILS
}