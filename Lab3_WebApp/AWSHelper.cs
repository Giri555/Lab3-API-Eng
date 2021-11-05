using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Lab3_WebApp
{
    public class AWSHelper
    {
        public static async Task UploadFileAsync(IFormFile videoUploaded, string key, string bucketname, IAmazonS3 S3Client)
        {
            try
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    videoUploaded.CopyTo(newMemoryStream);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = key,
                        BucketName = bucketname,
                        CannedACL = S3CannedACL.PublicRead
                    };
                    var fileTransferUtility = new TransferUtility(S3Client);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }
            }

            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {

                }
                else
                {
                    
                }
            }
        }

        public static void DownloadFile(IAmazonS3 S3Client, string bucketname, string videoKey)
        {
            try
            {
                TransferUtility fileTransferUtility = new TransferUtility(S3Client);
                string filename = "E:\\Downloads" + "\\" + videoKey;
                FileStream fs = File.Create(filename);
                fs.Close();
                fileTransferUtility.Download(filename, bucketname, videoKey);
            }
            catch (Exception)
            {
              
            }
        }
    }
}
