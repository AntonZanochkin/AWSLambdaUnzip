using System.IO.Compression;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
// ReSharper disable ConvertToUsingDeclaration

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaUnzip;

public class Function
{
    IAmazonS3 S3Client { get; }
    public Function()
    {
        S3Client = new AmazonS3Client();
    }
    public Function(IAmazonS3 s3Client)
    {
        S3Client = s3Client;
    }

    public async Task<string?> FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
      context.Logger.LogInformation($"IncomeRecordsCount:{s3Event.Records.Count}");

      foreach (var record in s3Event.Records)
      {
        var bucketName = record.S3.Bucket.Name;
        var key = record.S3.Object.Key;

        context.Logger.LogInformation($"IncomeRecord: Bucket{bucketName} EventName:{record.EventName} Key:{key}");

        if (record.EventName != "ObjectCreated:Put")
          continue;

        if (!key.EndsWith(".zip"))
          continue;

        var request = new GetObjectRequest { BucketName = bucketName, Key = key };

        try
        {
          using (var response = await S3Client.GetObjectAsync(request))
          {
            using (var zip = new ZipArchive(response.ResponseStream, ZipArchiveMode.Read))
            {
              var transferUtility = new TransferUtility(S3Client);
              foreach (var entry in zip.Entries)
              {
                if (string.IsNullOrEmpty(entry.Name)) continue;

                await using (var fileStream = entry.Open())
                {
                  using (var memoryStream = new MemoryStream())
                  {
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var destinationKey = Path.Combine("unzipped", entry.FullName);
                    await transferUtility.UploadAsync(memoryStream, bucketName, destinationKey);
                    context.Logger.LogInformation($"Unzipped: {bucketName}{destinationKey}");
                  }
                }
              }
            }
          }
        }
        catch (Exception e)
        {
          context.Logger.LogError($"Error getting object {key} from bucket {bucketName}");
          context.Logger.LogError(e.Message);
          context.Logger.LogError(e.StackTrace);
          throw;
        }
      }

      return null;
    }
}
