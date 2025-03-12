
locals {
    dns_zones_to_protect_against_email_spoofing = (
      var.prevent_email_spoofing ?
      tomap({
          zone_1 = data.aws_route53_zone.route_53_zone_for_our_domain,
      })
      : tomap({})
    )
}

resource "aws_route53_record" "dns_record_to_protect_against_email_spoofing__SPF" {
  for_each = local.dns_zones_to_protect_against_email_spoofing

  type = "TXT"
  name = each.value.name  // No subdomain
  records = ["v=spf1 -all"]
  ttl = 60
  zone_id = each.value.zone_id
}

resource "aws_route53_record" "dns_record_to_protect_against_email_spoofing__DMARC" {
  for_each = local.dns_zones_to_protect_against_email_spoofing

  type = "TXT"
  name = "_dmarc.${each.value.name}"
  records = ["v=DMARC1;p=reject;sp=reject;adkim=s;aspf=s;fo=1;rua=mailto:dmarc-rua@dmarc.service.gov.uk"]
  ttl = 60
  zone_id = each.value.zone_id
}

resource "aws_route53_record" "dns_record_to_protect_against_email_spoofing__DKIM" {
  for_each = local.dns_zones_to_protect_against_email_spoofing

  type = "TXT"
  name = "*._domainkey.${each.value.name}"
  records = ["v=DKIM1; p="]
  ttl = 60
  zone_id = each.value.zone_id
}

resource "aws_route53_record" "dns_record_to_protect_against_email_spoofing__MX" {
  for_each = local.dns_zones_to_protect_against_email_spoofing

  type = "MX"
  name = each.value.name  // No subdomain
  records = ["0 ."]
  ttl = 60
  zone_id = each.value.zone_id
}
