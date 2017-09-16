# Look Ma, No Servers!

This is the demo code for my talk on serverless applications.

## Getting Started

Detailed instructions on install/compile/deploy coming soon. If you attended the talk, it should be pretty straightforward.

### Prerequisites

This demo requires two files to exit in the directory *just above* this one. Those are:

* `auth0.json` - Your auth0 configuration file
* `private.key` - Your auth0 signing certificate

The `NoServers.Aws` project contains readme files that explain both of these files and how to create them.

### Basic instructions

* In the `NoServers.Aws` directory, update `aws-lambda-tools-defaults.json` with your information
* In the `NoServers.DataAccess.Aws.Test` directory update `appsettings.json` with your information
* In the `NoServers.Aws` directory, update `serverless.template` with your information. Specifically, you need to update the domain name for your CloudFront distribution and the SSL certificate.
* Create `../auth0.json`
* Create `../private.key`
* In the `NoServers.Web` directory, run `npm install`
* In the `NoServers.Web` directory, run `npm run build`
* In the `NoServers.Web/scripts` directory, edit the `auth0ClientConfig` with your information
* Build
* In the `NoServers.Aws` directory, run `dotnet lambda deploy-serverless`
* Copy the contents of `NoServers.Web/wwwroot` to the S3 bucket attached to your CloudFront distribution.
 