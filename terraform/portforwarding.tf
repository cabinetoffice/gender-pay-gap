//  Additional listener rules set on elastic beanstalk load balancer. Other rules set within ELB settings.

data "aws_lb" "load-balancer" {
  arn = aws_elastic_beanstalk_environment.gpg-elb-environment.load_balancers[0]
}

resource "aws_lb_listener" "port_80_listener" {
  load_balancer_arn = data.aws_lb.load-balancer.arn
  port              = "80"

  default_action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = "Not Authorised"
      status_code  = "401"
    }
  }
}

resource "aws_lb_listener_rule" "redirect_cloudfront_only_to_443" {
  listener_arn = aws_lb_listener.port_80_listener.arn

  action {
    type = "redirect"

    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
    }
  }

  // this header restricts access to load balancer to requests that include our custom header
  condition {
    http_header {
      http_header_name = "X-Custom-Header"
      values           = [random_integer.load-balancer-custom-header.id]
    }
  }
}