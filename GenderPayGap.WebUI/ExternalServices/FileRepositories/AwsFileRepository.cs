﻿using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace GenderPayGap.WebUI.ExternalServices.FileRepositories
{
    public class AwsFileRepository : IFileRepository
    {

        private readonly string bucketName;
        private readonly string awsAccessKeyId;
        private readonly string awsSecretAccessKey;
        private readonly string awsRegion;

        public AwsFileRepository(
            string bucketName,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            string awsRegion)
        {
            this.bucketName = bucketName;
            this.awsAccessKeyId = awsAccessKeyId;
            this.awsSecretAccessKey = awsSecretAccessKey;
            this.awsRegion = awsRegion;
        }


        public void Write(string relativeFilePath, string fileContents)
        {
            using (AmazonS3Client client = CreateAmazonS3Client())
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = DirToUrlSeparator(relativeFilePath),
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
                    BucketName = bucketName,
                    Key = DirToUrlSeparator(relativeFilePath),
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
                    BucketName = bucketName,
                    Key = DirToUrlSeparator(relativeFilePath)
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
                    BucketName = bucketName,
                    Prefix = DirToUrlSeparator(relativeDirectoryPath),
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
                    BucketName = bucketName,
                    Key = DirToUrlSeparator(relativeFilePath)
                };

                client.DeleteObjectAsync(request).Wait();
            }
        }

        public bool FileExists(string relativeFilePath)
        {
            using (AmazonS3Client client = CreateAmazonS3Client())
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = DirToUrlSeparator(relativeFilePath)
                };

                try
                {
                    client.GetObjectMetadataAsync(request).Wait();
                    return true;
                }
                catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
            }
        }

        public long? GetFileSize(string relativeFilePath)
        {
            using (AmazonS3Client client = CreateAmazonS3Client())
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = DirToUrlSeparator(relativeFilePath)
                };

                try
                {
                    GetObjectMetadataResponse response = client.GetObjectMetadataAsync(request).Result;
                    return response.ContentLength;
                }
                catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
            }
        }

        private AmazonS3Client CreateAmazonS3Client()
        {
            var amazonS3Client = new AmazonS3Client();

            return amazonS3Client;
        }

        private static string DirToUrlSeparator(string filePath)
        {
            return filePath?.Replace('\\', '/');
        }

    }
}
