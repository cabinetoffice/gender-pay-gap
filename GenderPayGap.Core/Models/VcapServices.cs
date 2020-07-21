using System.Collections.Generic;
using GenderPayGap.Extensions.AspNetCore;
using Newtonsoft.Json;

namespace GenderPayGap.Core.Models
{
    public class VcapServices
    {

        [JsonProperty("aws-s3-bucket")]
        public List<VcapAwsS3Bucket> AwsS3Bucket { get; set; }

        [JsonProperty("redis")]
        public List<VcapRedis> Redis { get; set; }

        [JsonProperty("postgres")]
        public List<VcapPostgres> Postgres { get; set; }

    }

    public class VcapAwsS3Bucket
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("credentials")]
        public VcapAwsS3Credentials Credentials { get; set; }

    }

    public class VcapAwsS3Credentials
    {

        [JsonProperty("bucket_name")]
        public string BucketName { get; set; }

        [JsonProperty("aws_access_key_id")]
        public string AwsAccessKeyId { get; set; }

        [JsonProperty("aws_secret_access_key")]
        public string AwsSecretAccessKey { get; set; }

        [JsonProperty("aws_region")]
        public string Region { get; set; }

    }

    public class VcapRedis
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("credentials")]
        public VcapRedisCredentials Credentials { get; set; }

    }

    public class VcapRedisCredentials
    {

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("tls_enabled")]
        public bool TlsEnabled { get; set; }

    }

    public class VcapPostgres
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("credentials")]
        public VcapPostgresCredentials Credentials { get; set; }

        public string GetConnectionString()
        {
            return $"Server={Credentials.Host};"
                   + $"Port={Credentials.Port};"
                   + $"Database={Credentials.DatabaseName};"
                   + $"User Id={Credentials.Username};"
                   + $"Password={Credentials.Password};"
                   + (Config.IsLocal()
                       ? ""
                       : "SslMode=Require;Trust Server Certificate=true");
        }

    }

    public class VcapPostgresCredentials
    {

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("name")]
        public string DatabaseName { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

    }
}
