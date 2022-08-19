// declarations for the resources to be created 
// this does not include the state S3 bucket as this must be created in order for terraform to init

resource "aws_security_group" "allow_postgres_connection" {
  name        = "allow_postgres_connection"
  description = "Allow Postgres DB inbound traffic"
  vpc_id      = "vpc-0e9491851e4208470"

  tags = {
    Name = "allow_postgres_connection"
  }
}

resource "aws_security_group_rule" "postgres_in" {
  security_group_id = aws_security_group.allow_postgres_connection.id
  type              = "ingress"
  from_port         = 5432
  to_port           = 5432
  protocol          = "tcp"
  cidr_blocks = ["0.0.0.0/0"]
}

resource "aws_security_group_rule" "postgres_out" {
  security_group_id = aws_security_group.allow_postgres_connection.id
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks = ["0.0.0.0/0"]
}

resource "aws_db_instance" "gpg-dev-db" {
  allocated_storage           = 100
  engine                      = "postgres"
  engine_version              = "14"
  instance_class              = "db.t3.small"
  identifier                  = "gpg-dev-db"
  db_name                     = "gpgDevDb"
  username                    = "postgres"
  password                    = "postgres"
  port                        = 5432
  backup_retention_period     = 5
  backup_window               = "04:00-05:00"
  vpc_security_group_ids      = [aws_security_group.allow_postgres_connection.id]
  storage_encrypted           = true
  publicly_accessible         = true
  allow_major_version_upgrade = true
}