resource "aws_iam_role" "aws-elasticbeanstalk-ec2-role" {
  name                = "aws-elasticbeanstalk-ec2-role-${var.env}"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = [	
            "ec2.amazonaws.com",
            "elasticbeanstalk.amazonaws.com"
          ]
        }
      },
    ]
  })  
  managed_policy_arns = ["arn:aws:iam::aws:policy/AmazonRDSFullAccess", "arn:aws:iam::aws:policy/AmazonElastiCacheFullAccess", "arn:aws:iam::aws:policy/AWSElasticBeanstalkWebTier","arn:aws:iam::aws:policy/AWSElasticBeanstalkMulticontainerDocker","arn:aws:iam::aws:policy/AmazonSSMManagedEC2InstanceDefaultPolicy", "arn:aws:iam::aws:policy/AWSElasticBeanstalkWorkerTier", "arn:aws:iam::aws:policy/AmazonSSMManagedEC2InstanceDefaultPolicy"]
}
