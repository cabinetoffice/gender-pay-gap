// hosted zone
resource "aws_route53_zone" "prod" {
  provider = aws.us-east-1
  name     = var.route_53_domain
}

// Alias record for domain

resource "aws_route53_record" "root_ipv4" {
  zone_id = aws_route53_zone.prod.zone_id
  name    = var.route_53_domain
  type    = "A"

  alias {
    name                   = aws_cloudfront_distribution.gpg-distribution.domain_name
    zone_id                = aws_cloudfront_distribution.gpg-distribution.hosted_zone_id
    evaluate_target_health = false
  }
}

// AAAA Alias record for ipV6 domain

resource "aws_route53_record" "root_ipv6" {
  zone_id = aws_route53_zone.prod.zone_id
  name    = var.route_53_domain
  type    = "AAAA"

  alias {
    name                   = aws_cloudfront_distribution.gpg-distribution.domain_name
    zone_id                = aws_cloudfront_distribution.gpg-distribution.hosted_zone_id
    evaluate_target_health = false
  }
}

// add subdomain for genderpaygap