using System.Collections.Generic;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.ExternalServices.FileRepositories
{
    public class AwsFileRepository : IFileRepository
    {

        private readonly VcapAwsS3Bucket vcapAwsS3Bucket;

        public AwsFileRepository(VcapAwsS3Bucket vcapAwsS3Bucket)
        {
            this.vcapAwsS3Bucket = vcapAwsS3Bucket;
        }


        public void Write(string relativeFilePath, string fileContents)
        {
            using (AmazonS3Client client = CreateAmazonS3Client())
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = vcapAwsS3Bucket.Credentials.BucketName,
                    Key = Url.DirToUrlSeparator(relativeFilePath),
                    ContentBody = fileContents
                };

                client.PutObjectAsync(putRequest).Wait();
            }
        }

        public void Write(string relativeFilePath, byte[] fileContents)
        {
            using (AmazonS3Client client = CreateAmazonS3Client())
            {
                var memoryStream = new MemoryStream();
                memoryStream.Write(fileContents);

                var putRequest = new PutObjectRequest
                {
                    BucketName = vcapAwsS3Bucket.Credentials.BucketName,
                    Key = Url.DirToUrlSeparator(relativeFilePath),
                    InputStream = memoryStream
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

        public List<string> GetFiles(string relativeDirectoryPath)
        {
            using (AmazonS3Client client = CreateAmazonS3Client())
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = vcapAwsS3Bucket.Credentials.BucketName,
                    Prefix = Url.DirToUrlSeparator(relativeDirectoryPath),
                    MaxKeys = 10000
                };

                var filePaths = new List<string>();
                ListObjectsV2Response response;
                do
                {
                    response = client.ListObjectsV2Async(request).Result;

                    foreach (S3Object entry in response.S3Objects)
                    {
                        string fileNameWithDirectory = entry.Key;

                        string fileNameWithoutDirectory =
                            fileNameWithDirectory.StartsWith(relativeDirectoryPath)
                                ? fileNameWithDirectory.Substring(relativeDirectoryPath.Length + 1)
                                : fileNameWithDirectory;

                        filePaths.Add(fileNameWithoutDirectory);
                    }
                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                return filePaths;
            }
        }

        public void Delete(string relativeFilePath)
        {
            using (AmazonS3Client client = CreateAmazonS3Client())
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = vcapAwsS3Bucket.Credentials.BucketName,
                    Key = Url.DirToUrlSeparator(relativeFilePath)
                };

                client.DeleteObjectAsync(request).Wait();
            }
        }

        private AmazonS3Client CreateAmazonS3Client()
        {
            string accessKey = vcapAwsS3Bucket.Credentials.AwsAccessKeyId;
            string secretKey = vcapAwsS3Bucket.Credentials.AwsSecretAccessKey;

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var amazonS3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(vcapAwsS3Bucket.Credentials.Region));

            return amazonS3Client;
        }

    }
}
