using System.IO;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes
{
    public class AwsFileRepository : IFileRepository
    {

        private readonly VcapAwsS3Bucket vcapAwsS3Bucket;

        public AwsFileRepository(VcapAwsS3Bucket vcapAwsS3Bucket)
        {
            this.vcapAwsS3Bucket = vcapAwsS3Bucket;
        }


        public void Write(string relativeFilePath, string csvFileContents)
        {
            using (AmazonS3Client client = CreateAmazonS3Client())
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = vcapAwsS3Bucket.Credentials.BucketName,
                    Key = Url.DirToUrlSeparator(relativeFilePath),
                    ContentBody = csvFileContents
                };

                client.PutObjectAsync(putRequest).Wait();
            }
        }

        public string Read(string relativeFilePath)
        {
            using (AmazonS3Client client = CreateAmazonS3Client())
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = vcapAwsS3Bucket.Credentials.BucketName,
                    Key = Url.DirToUrlSeparator(relativeFilePath)
                };

                using (GetObjectResponse response = client.GetObjectAsync(request).Result)
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string csvFileContents = reader.ReadToEnd();
                    return csvFileContents;
                }
            }
        }

        private AmazonS3Client CreateAmazonS3Client()
        {
            string accessKey = vcapAwsS3Bucket.Credentials.AwsAccessKeyId;
            string secretKey = vcapAwsS3Bucket.Credentials.AwsSecretAccessKey;

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var amazonS3Client = new AmazonS3Client(credentials);

            return amazonS3Client;
        }

    }
}
