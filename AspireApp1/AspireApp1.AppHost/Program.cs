using Amazon;
using Aspire.Hosting.LocalStack.Container;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("project-cache")
    .WithRedisInsight(containerName: "project-insight");

var gateway = builder.AddProject<Projects.Api_Gateway>("api-gateway");

var awsConfig = builder.AddAWSSDKConfig()
    .WithProfile("default")
    .WithRegion(RegionEndpoint.EUCentral1);

var localstack = builder
    .AddLocalStack("project-localstack", awsConfig: awsConfig, configureContainer: container =>
    {
        container.Lifetime = ContainerLifetime.Session;
        container.DebugLevel = 1;
        container.LogLevel = LocalStackLogLevel.Debug;
        container.Port = 4566;
        container.AdditionalEnvironmentVariables.Add("DEBUG", "1");
    });

var awsResources = builder
    .AddAWSCloudFormationTemplate("resources", "CloudFormation/programproject-template-sqs-s3.yaml", "programproject")
    .WithReference(awsConfig);

for (var i = 0; i < 5; i++)
{
    var service = builder.AddProject<Projects.ServiceApi>($"programproject-api-{i + 1}", launchProfileName: null)
        .WithHttpsEndpoint(4440 + i)
        .WithReference(cache, "RedisCache")
        .WithReference(awsResources)
        .WaitFor(cache)
        .WaitFor(awsResources);
    gateway.WaitFor(service);
}

builder.AddProject<Projects.Client_Wasm>("programproject-wasm")
    .WaitFor(gateway);

builder.AddProject<Projects.FileService>("fileservice")
    .WithReference(awsResources)
    .WaitFor(awsResources);

builder.UseLocalStack(localstack);

builder.Build().Run();
