
////////////////////////////////////////////////
// The HTTPS certificate for the main website

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


///////////////////////////////////////////////////////
// The HTTPS certificate for the www domain redirect

resource "aws_acm_certificate" "https_certificate__www_domain_redirect" {
  count = (var.create_redirect_from_www_domain) ? 1 : 0  // Only create this HTTPS Certificate if "var.create_redirect_from_www_domain" is true

  // This certificate is for use by CloudFront, so it has to be created in the us-east-1 region (for some reason!)
  provider = aws.us-east-1

  domain_name = "${var.dns_record_www_domain_including_dot}${data.aws_route53_zone.route_53_zone_for_our_domain.name}"
  validation_method = "DNS"
}

locals {
  dns_records_we_need_to_verify_www_domain__list = flatten([
    for https_certificate in aws_acm_certificate.https_certificate__www_domain_redirect : [
      for dvo in https_certificate.domain_validation_options : {
        domain_name = dvo.domain_name
        name        = dvo.resource_record_name
        record      = dvo.resource_record_value
        type        = dvo.resource_record_type
      }
    ]
  ])
  dns_records_we_need_to_verify_www_domain__map = {
    for i, record in local.dns_records_we_need_to_verify_www_domain__list: record.domain_name => record
  }
}

resource "aws_route53_record" "dns_records_for_https_certificate_verification__www_domain_redirect" {
  for_each = local.dns_records_we_need_to_verify_www_domain__map

  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = data.aws_route53_zone.route_53_zone_for_our_domain.zone_id
}

resource "aws_acm_certificate_validation" "certificate_validation_waiter__www_domain_redirect" {
  count = (var.create_redirect_from_www_domain) ? 1 : 0  // Only create this HTTPS Certificate if "var.create_redirect_from_www_domain" is true

  // This certificate is for use by CloudFront, so it has to be created in the us-east-1 region (for some reason!)
  provider = aws.us-east-1

  certificate_arn = aws_acm_certificate.https_certificate__www_domain_redirect[0].arn
  validation_record_fqdns = [for record in aws_route53_record.dns_records_for_https_certificate_verification__www_domain_redirect : record.fqdn]
}
