
resource "aws_acm_certificate" "https_certificate_for_our_domain" {
  // This certificate is for use by CloudFront, so it has to be created in the us-east-1 region (for some reason!)
  provider = aws.us-east-1

  domain_name = "${var.dns_record_subdomain_including_dot}${data.aws_route53_zone.route_53_zone_for_our_domain.name}"
  validation_method = "DNS"
}

resource "aws_route53_record" "example" {
  for_each = {
    for dvo in aws_acm_certificate.https_certificate_for_our_domain.domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  }

  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = data.aws_route53_zone.route_53_zone_for_our_domain.zone_id
}

resource "aws_acm_certificate_validation" "certificate_validation_waiter" {
  // This certificate is for use by CloudFront, so it has to be created in the us-east-1 region (for some reason!)
  provider = aws.us-east-1

  certificate_arn = aws_acm_certificate.https_certificate_for_our_domain.arn
  validation_record_fqdns = [for record in aws_route53_record.example : record.fqdn]
}
