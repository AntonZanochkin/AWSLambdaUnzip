AWS Lambda Unzip
This is a simple AWS Lambda function written in C# that listens to S3 events and unzips any uploaded zip files, uploading the contents to the specified destination directory.

Prerequisites
.NET Core SDK
AWS account with appropriate permissions for Lambda and S3
Setup
Clone this repository to your local machine.
Build the project using dotnet build.
Deploy the Lambda function to your AWS account using the AWS CLI or AWS Management Console.
Configure the necessary environment variables:
DestinationDirectory: The directory where unzipped files will be stored.
Usage
Once the Lambda function is deployed and configured, it will automatically listen to S3 events and process any uploaded zip files accordingly.

Configuration
The Lambda function can be configured using environment variables:

DestinationDirectory: The directory where unzipped files will be stored.

License
This project is licensed under the MIT License.
