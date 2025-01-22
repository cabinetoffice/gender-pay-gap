
resource "aws_security_group" "security_group_database" {
  name = "${var.service_name}__${var.environment}__Security_Group__Database"
  description = "Database security group (${var.service_name}/${var.environment})"
  vpc_id = aws_vpc.vpc_main.id

  tags = {
    Name = "${var.service_name}__${var.environment}__Security_Group__Database"
  }
}

resource "aws_security_group_rule" "security_group_database__ingress_port80_ec2_instances" {
  security_group_id = aws_security_group.security_group_database.id
  type              = "ingress"
  description       = "Allow ingress: Port 5432 from EC2 instances"
  protocol          = "tcp"
  from_port         = 5432
  to_port           = 5432
  source_security_group_id = aws_security_group.security_group_main_app_instances.id
}

resource "aws_security_group_rule" "security_group_database__egress_all" {
  security_group_id = aws_security_group.security_group_database.id
  type              = "egress"
  description       = "Allow egress: all"
  protocol          = "-1"
  from_port         = 0
  to_port           = 0
  cidr_blocks       = ["0.0.0.0/0"]
}
