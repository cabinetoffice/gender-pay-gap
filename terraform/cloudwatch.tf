resource "aws_cloudwatch_dashboard" "GPG_Dashboard" {
  dashboard_name = "${var.env}-EC2-Dashboard"

  dashboard_body = <<EOF
{
    "widgets": [
        {
            "type": "explorer",
            "width": 24,
            "height": 15,
            "x": 0,
            "y": 0,
            "properties": {
                "metrics": [
                    {
                        "metricName": "CPUUtilization",
                        "resourceType": "AWS::EC2::Instance",
                        "stat": "Maximum"
                    }
                ],
                "aggregateBy": {
                    "key": "InstanceType",
                    "func": "MAX"
                },
                "labels": [
                    {
                        "key": "State",
                        "value": "running"
                    }
                ],
                "widgetOptions": {
                    "legend": {
                        "position": "bottom"
                    },
                    "view": "timeSeries",
                    "rowsPerPage": 8,
                    "widgetsPerRow": 2
                },
                "period": 60,
                "title": "Running EC2 Instances CPUUtilization"
            }
        }
    ]
}
EOF
}

// Alarms

resource "aws_cloudwatch_metric_alarm" "no_healthy_hosts" {
  alarm_name          = "${var.env}-no-healthy-hosts"
  metric_name         = "HealthyHostCount"
  namespace           = "AWS/ApplicationELB"
  comparison_operator = "LessThanOrEqualToThreshold"
  statistic           = "Minimum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  alarm_description   = "gpg-${var.env} has no healthy hosts."
  alarm_actions       = [aws_sns_topic.EC2_topic]
  ok_actions          = [aws_sns_topic.EC2_topic]
  dimensions = {
    LoadBalancer = data.aws_lb.load-balancer.arn
  }
}

resource "aws_cloudwatch_metric_alarm" "http_errors" {
  alarm_name          = "${var.env}-http-errors"
  metric_name         = "HTTPCode_Target_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  comparison_operator = "GreaterThanThreshold"
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  alarm_description   = "gpg-${var.env} has HTTP 5xx errors."
  alarm_actions       = [aws_sns_topic.EC2_topic]
  ok_actions          = [aws_sns_topic.EC2_topic]
  treat_missing_data  = "notBreaching"
  dimensions = {
    LoadBalancer = data.aws_lb.load-balancer.arn
  }
}

# No instances should fail the health checks unless they failed to boot.
# This usually means a release failed and will need manual intervention.
resource "aws_cloudwatch_metric_alarm" "unhealthy_hosts" {
  alarm_name          = "${var.env}-unhealthy-hosts"
  metric_name         = "UnHealthyHostCount"
  namespace           = "AWS/ApplicationELB"
  comparison_operator = "GreaterThanThreshold"
  statistic           = "Maximum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 0
  alarm_description   = "gpg-${var.env} has unhealthy hosts."
  alarm_actions       = [aws_sns_topic.EC2_topic]
  ok_actions          = [aws_sns_topic.EC2_topic]
  treat_missing_data  = "notBreaching"
  dimensions = {
    LoadBalancer = data.aws_lb.load-balancer.arn
  }
}

resource "aws_cloudwatch_metric_alarm" "cpu_utilisation" {
  alarm_name          = "${var.env}-cpu-too-high"
  metric_name = "CPUUtilization"
  namespace = "AWS/EC2"
  comparison_operator = "GreaterThanOrEqualToThreshold"
  statistic = "Average"
  period    = "60"
  evaluation_periods  = "2"
  threshold         = "70"
  alarm_actions       = [aws_sns_topic.EC2_topic]
  ok_actions          = [aws_sns_topic.EC2_topic]
  treat_missing_data  = "notBreaching"
  alarm_description = "This metric monitors ec2 cpu utilization exceeding 70%"
}

resource "aws_sns_topic" "EC2_topic" {
  name = "elb-${var.env}-cloudwatch-alarms "
}

resource "aws_cloudwatch_composite_alarm" "EC2" {
  alarm_description = "Composite alarm"
  alarm_name        = "EC2_Composite_Alarm"
  alarm_actions     = [aws_sns_topic.EC2_topic]

  alarm_rule = "ALARM(${aws_cloudwatch_metric_alarm.cpu_utilisation}) OR ALARM(${aws_cloudwatch_metric_alarm.http_errors}) OR ALARM(${aws_cloudwatch_metric_alarm.no_healthy_hosts}) OR ALARM(${aws_cloudwatch_metric_alarm.unhealthy_hosts}) "

  depends_on = [
    aws_cloudwatch_metric_alarm.cpu_utilisation,
    aws_cloudwatch_metric_alarm.http_errors,
    aws_cloudwatch_metric_alarm.no_healthy_hosts,
    aws_cloudwatch_metric_alarm.unhealthy_hosts,
    aws_sns_topic.EC2_topic,
    aws_sns_topic_subscription.EC2_Subscription
  ]
}

resource "aws_sns_topic_subscription" "EC2_Subscription" {
  topic_arn = aws_sns_topic.EC2_topic.arn
  protocol  = "email"
  endpoint  = var.cloudwatch_notification_emails
}